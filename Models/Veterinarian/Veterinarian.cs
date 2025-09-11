using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCare.Models.Veterinarian
{
    public enum VetStatus
    {
        Pending = 0,   // Default status after registration
        Approved = 1,  // Can log in
        Rejected = 2   // Blocked from login
    }

    public class Veterinarian
    {
        [Key]
        public int VetId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name must contain only letters")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Contact Number is required")]
        [RegularExpression(@"^[0-9]{10,15}$", ErrorMessage = "Contact number must be 10 to 15 digits")]
        public string Contact { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(100, ErrorMessage = "Address cannot exceed 100 characters")]
        public string Address { get; set; }

        // Plain password for input only (not saved to DB)
        [NotMapped]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        // Hashed password stored in DB
  
        public string? PasswordHash { get; set; }

        [StringLength(100)]
        public string Specialization { get; set; }

        [Range(0, 50, ErrorMessage = "Experience must be between 0 and 50 years")]
        public int? Experience { get; set; }

        [StringLength(200)]
        public string? AvailableSlots { get; set; }

        public VetStatus Status { get; set; } = VetStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
