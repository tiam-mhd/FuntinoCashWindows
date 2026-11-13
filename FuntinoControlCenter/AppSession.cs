namespace FuntinoControlCenter
{
    internal sealed class AppSession
    {
        public int UserId { get; private set; }
        public string Username { get; private set; }
        public bool IsAdmin { get; private set; }
        public bool MustChangePassword { get; private set; }
        public static AppSession Current { get; private set; }

        public AppSession(int userId, string username, bool isAdmin, bool mustChangePassword)
        {
            UserId = userId; Username = username; IsAdmin = isAdmin; MustChangePassword = mustChangePassword;
            Current = this;
        }
        public static void Clear() { Current = null; }
    }
}
