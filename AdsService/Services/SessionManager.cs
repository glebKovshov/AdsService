namespace AdsService.Services
{
    public static class SessionManager
    {
        public static Users CurrentUser { get; private set; }

        public static bool IsAuthorized
        {
            get
            {
                return CurrentUser != null;
            }
        }

        public static void SignIn(Users user)
        {
            CurrentUser = user;
        }

        public static void SignOut()
        {
            CurrentUser = null;
        }
    }
}
