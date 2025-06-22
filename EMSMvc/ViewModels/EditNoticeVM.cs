using System.ComponentModel.DataAnnotations;

namespace EMSMvc.ViewModels
{
    public class EditNoticeVM : CreateNoticeVM
    {
        [Required]
        public Guid Id { get; set; }
    }
}
