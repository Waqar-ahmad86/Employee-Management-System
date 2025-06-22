namespace EMSMvc.Core.Application.DTOs
{
    public class ApiErrorResponse
    {
        public string Message { get; set; }
        public bool IsAdminLocked { get; set; }
    }
}
