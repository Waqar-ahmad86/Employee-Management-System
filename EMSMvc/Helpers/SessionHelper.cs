namespace EMSMvc.Helpers
{
    public static class SessionHelper
    {
        public static bool IsUserLoggedIn(HttpContext? context)
        {
            return !string.IsNullOrEmpty(context?.Session.GetString("JwtToken"));
        }

        public static bool IsAdmin(HttpContext? context)
        {
            return context?.Session.GetString("UserRole") == "Admin";
        }
    }
}
