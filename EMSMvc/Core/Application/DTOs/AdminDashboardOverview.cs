namespace EMSMvc.Core.Application.DTOs
{
    public class AdminDashboardOverview
    {
        public int TotalActiveUsers { get; set; }
        public int TotalEmployees { get; set; }
        public int UsersCheckedInToday { get; set; }
    }
}