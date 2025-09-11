using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCare.Models.Veterinarian
{
    public class PetRecord
    {
        [Key]
        public int PetRecordId { get; set; }

        [Required]
        public int VetId { get; set; }

        [Required]
        public int PetId { get; set; }

        [Required(ErrorMessage = "Pet's name is required")]
        [StringLength(50)]
        public string PetName { get; set; }

        [StringLength(100)]
        public string Breed { get; set; }

        [Range(0, 100)]
        public int? Age { get; set; }  // Age in years

        [StringLength(500)]
        public string MedicalHistory { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("VetId")]
        public virtual Veterinarian Veterinarian { get; set; }

        // Assume Pet model exists
        // [ForeignKey("PetId")]
        // public virtual Pet Pet { get; set; }
    }
}
