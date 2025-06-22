namespace EMSMvc.Core.Application.DTOs
{
    public class ExternalLoginRequest
    {
        public string Provider { get; set; }
        public string ProviderKey { get; set; }
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
    }
}
