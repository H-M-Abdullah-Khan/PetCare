using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCare.Models.Veterinarian
{
    public enum AppointmentStatus
    {
        Scheduled = 0,
        Completed = 1,
        Cancelled = 2
    }

    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int VetId { get; set; }

        [Required]
        public int PetId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(100)]
        public string Reason { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("VetId")]
        public virtual Veterinarian Veterinarian { get; set; }

        // Assume Pet model exists with PetId
        // [ForeignKey("PetId")]
        // public virtual Pet Pet { get; set; }
    }
}
