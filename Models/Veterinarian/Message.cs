using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCare.Models.Veterinarian
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int RecipientVetId { get; set; }

        [Required]
        [StringLength(100)]
        public string SenderName { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        public DateTime DateSent { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        // Navigation property
        [ForeignKey("RecipientVetId")]
        public virtual Veterinarian RecipientVeterinarian { get; set; }
    }
}
