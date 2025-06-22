namespace EMSWebApi.Application.DTOs.Dashboard
{
    public class AdminDashboardOverviewDto
    {
        public int TotalActiveUsers { get; set; }
        public int TotalEmployees { get; set; }
        public int UsersCheckedInToday { get; set; }
    }
}
