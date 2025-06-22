namespace EMSMvc.Core.Application.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public List<string> Roles { get; set; }
    }
}