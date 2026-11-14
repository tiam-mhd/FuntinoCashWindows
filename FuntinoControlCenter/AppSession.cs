namespace FuntinoControlCenter
{
    internal sealed class AppSession
    {
        public int ApplicationUserId { get; private set; }
        public string UserName { get; private set; }
        public bool IsAdministrator { get; private set; }
        public bool MustChangePassword { get; private set; }
        public static AppSession Current { get; private set; }

        public AppSession(int applicationUserId, string userName, bool isAdministrator, bool mustChangePassword)
        {
            ApplicationUserId = applicationUserId; UserName = userName; IsAdministrator = isAdministrator; MustChangePassword = mustChangePassword;
            Current = this;
        }
        public static void Clear() { Current = null; }
    }
}
