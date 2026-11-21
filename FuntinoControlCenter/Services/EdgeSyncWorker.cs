using System;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using FuntinoControlCenter.Database;

namespace FuntinoControlCenter.Services
{
    internal static class EdgeSyncWorker
    {
        private const int BatchSize = 50;
        private const string WorkerMutexName = "Local\\FuntinoControlCenter.EdgeSync";
        private const string StopEventName = "Local\\FuntinoControlCenter.EdgeSync.Stop";

        public static void RequestStop()
        {
            try
            {
                using (var stop = EventWaitHandle.OpenExisting(StopEventName, EventWaitHandleRights.Modify))
                    stop.Set();
            }
            catch (WaitHandleCannotBeOpenedException) { }
            catch (UnauthorizedAccessException) { }

            try
            {
                using (var mutex = Mutex.OpenExisting(WorkerMutexName, MutexRights.Synchronize))
                {
                    bool acquired = false;
                    try { acquired = mutex.WaitOne(TimeSpan.FromSeconds(60)); }
                    catch (AbandonedMutexException) { acquired = true; }
                    if (acquired) mutex.ReleaseMutex();
                }
            }
            catch (WaitHandleCannotBeOpenedException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static int Run()
        {
            bool created;
            using (var mutex = CreateWorkerMutex(out created))
            {
                if (!created) return 0;
                using (var stopping = CreateStopEvent(out _))
                {
                    stopping.Reset();
                    EventHandler processExit = delegate { stopping.Set(); };
                    AppDomain.CurrentDomain.ProcessExit += processExit;
                    DateTime nextSnapshotUtc = DateTime.MinValue;
                    int snapshotFailures = 0;
                    var random = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));

                    try
                    {
                        using (EdgeWorkerContext.Enter())
                        {
                            while (!stopping.WaitOne(0))
                            {
                                TimeSpan wait = TimeSpan.FromSeconds(10);
                                try
                                {
                                    AppSettings settings = AppSettings.Load();
                                    var client = new EdgeApiClient(settings);
                                    using (client)
                                    {
                                        if (DateTime.UtcNow >= nextSnapshotUtc)
                                        {
                                            try
                                            {
                                                string cursor = EdgeIntegrationRepository.GetSnapshotCursor(settings.EdgeBranchId);
                                                EdgeBranchSnapshotDto snapshot = client.GetSnapshot(cursor);
                                                EdgeIntegrationRepository.ReplaceSnapshot(snapshot, settings.EdgeBranchId);
                                                snapshotFailures = 0;
                                                nextSnapshotUtc = GetNextSnapshotTime(snapshot);
                                                EdgeIntegrationRepository.SetTransportState(true, DateTime.UtcNow, null, false);
                                            }
                                            catch (EdgeApiException exception)
                                            {
                                                snapshotFailures++;
                                                nextSnapshotUtc = DateTime.UtcNow.AddSeconds(GetBackoff(snapshotFailures, random));
                                                EdgeIntegrationRepository.SetTransportState(false, null, exception.SafeCode, false);
                                            }
                                        }

                                        bool sentAny = false;
                                        foreach (bool deviceDatabase in new[] { false, true })
                                        {
                                            try
                                            {
                                                var batch = EdgeIntegrationRepository.GetPendingEvents(deviceDatabase, settings.EdgeBranchId, BatchSize);
                                                if (batch.Count == 0) continue;
                                                sentAny = true;
                                                EdgeIntegrationRepository.MarkBatchAttempt(batch);
                                                try
                                                {
                                                    EdgeAcknowledgementResponse response = client.SendEvents(batch.Select(row => row.Payload).ToList());
                                                    EdgeIntegrationRepository.ApplyAcknowledgements(batch, response);
                                                    EdgeIntegrationRepository.SetTransportState(true, DateTime.UtcNow, null, false);
                                                }
                                                catch (EdgeApiException exception)
                                                {
                                                    int retrySeconds = GetBackoff(batch, random);
                                                    EdgeIntegrationRepository.RecordBatchFailure(batch, exception.SafeCode, retrySeconds, !exception.Retryable);
                                                    EdgeIntegrationRepository.SetTransportState(false, exception.Retryable ? (DateTime?)null : DateTime.UtcNow, exception.SafeCode, false);
                                                }
                                            }
                                            catch (Exception exception)
                                            {
                                                EdgeIntegrationRepository.SetTransportState(false, null,
                                                    deviceDatabase ? "device-database-unavailable" : GetSafeDatabaseError(exception), false);
                                            }
                                        }

                                        EdgeIntegrationRepository.SaveHeartbeat();
                                        wait = sentAny ? TimeSpan.FromSeconds(1) : TimeSpan.FromSeconds(8);
                                    }
                                }
                                catch (EdgeApiException exception)
                                {
                                    SetOffline(exception.SafeCode);
                                    wait = TimeSpan.FromSeconds(GetBackoff(++snapshotFailures, random));
                                }
                                catch (Exception exception)
                                {
                                    SetOffline(GetSafeDatabaseError(exception));
                                    wait = TimeSpan.FromSeconds(15);
                                }
                                stopping.WaitOne(wait);
                            }
                        }
                    }
                    finally
                    {
                        AppDomain.CurrentDomain.ProcessExit -= processExit;
                        try { SetOffline("worker-stopped"); }
                        catch { }
                    }
                }
            }
            return 0;
        }

        private static EventWaitHandle CreateStopEvent(out bool created)
        {
            var security = new EventWaitHandleSecurity();
            var authenticatedUsers = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
            security.AddAccessRule(new EventWaitHandleAccessRule(authenticatedUsers,
                EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize, AccessControlType.Allow));
            return new EventWaitHandle(false, EventResetMode.ManualReset, StopEventName, out created, security);
        }

        private static Mutex CreateWorkerMutex(out bool created)
        {
            var security = new MutexSecurity();
            var authenticatedUsers = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
            security.AddAccessRule(new MutexAccessRule(authenticatedUsers,
                MutexRights.Modify | MutexRights.Synchronize, AccessControlType.Allow));
            return new Mutex(true, WorkerMutexName, out created, security);
        }

        private static void SetOffline(string safeCode)
        {
            try
            {
                using (EdgeWorkerContext.Enter())
                    EdgeIntegrationRepository.SetTransportState(false, null, safeCode, false);
            }
            catch { }
        }

        private static DateTime GetNextSnapshotTime(EdgeBranchSnapshotDto snapshot)
        {
            DateTimeOffset expires;
            if (!DateTimeOffset.TryParse(snapshot == null ? null : snapshot.ExpiresAt, out expires))
                return DateTime.UtcNow.AddMinutes(5);
            DateTime refreshBeforeExpiry = expires.UtcDateTime.AddMinutes(-5);
            DateTime periodicRefresh = DateTime.UtcNow.AddMinutes(10);
            DateTime next = refreshBeforeExpiry < periodicRefresh ? refreshBeforeExpiry : periodicRefresh;
            return next < DateTime.UtcNow.AddSeconds(30) ? DateTime.UtcNow.AddSeconds(30) : next;
        }

        private static int GetBackoff(System.Collections.Generic.IList<EdgeQueuedEvent> events, Random random)
        {
            int attempts = events == null || events.Count == 0 ? 1 : events.Max(row => row.RetryCount) + 1;
            return GetBackoff(attempts, random);
        }

        private static int GetBackoff(int attempts, Random random)
        {
            int power = Math.Max(0, Math.Min(10, attempts - 1));
            double seconds = Math.Min(3600d, 5d * Math.Pow(2d, power));
            int basis = (int)seconds;
            int jitter = random.Next(-Math.Max(1, basis / 5), Math.Max(2, basis / 5 + 1));
            return Math.Max(1, Math.Min(3600, basis + jitter));
        }

        private static string GetSafeDatabaseError(Exception exception)
        {
            var sql = exception as System.Data.SqlClient.SqlException;
            if (sql != null)
            {
                if (sql.Number == 4060 || sql.Number == 18456 || sql.Number == 53 || sql.Number == -1)
                    return "operational-database-unavailable";
                return "operational-database-error";
            }
            return "worker-error";
        }
    }
}
