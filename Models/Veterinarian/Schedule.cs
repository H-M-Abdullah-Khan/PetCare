using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCare.Models.Veterinarian
{
    public class Schedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        public int VetId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string TimeSlot { get; set; }  // e.g. "09:00 AM - 10:00 AM"

        [StringLength(200)]
        public string Notes { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation property
        [ForeignKey("VetId")]
        public virtual Veterinarian Veterinarian { get; set; }
    }
}
