namespace EMSWebApi.Application.DTOs.Leave
{
    public class LeaveTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int? DefaultDaysAllowed { get; set; }
        public bool IsActive { get; set; }
    }
}
