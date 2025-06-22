namespace EMSMvc.ViewModels
{
    public class EditUserRolesVM
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public List<string> AllRoles { get; set; } = new List<string>();
        public List<string> UserRoles { get; set; } = new List<string>();
    }
}
