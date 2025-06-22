using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EMSWebApi.Domain.Identity;
using EMS.Common.Enums;

namespace EMSWebApi.Domain.Entities
{
    public class AttendanceRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? CheckInTime { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? CheckOutTime { get; set; }

        public AttendanceStatus Status { get; set; }

        public double? WorkHours
        {
            get
            {
                if (CheckInTime.HasValue && CheckOutTime.HasValue)
                {
                    return (CheckOutTime.Value - CheckInTime.Value).TotalHours;
                }
                return null;
            }
        }

        [StringLength(200)]
        public string? Remarks { get; set; }
    }
}
