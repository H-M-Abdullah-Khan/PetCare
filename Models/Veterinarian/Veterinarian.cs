using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.Veterinarian
{
    /// <summary>
    /// Enum for Veterinarian Account Status
    /// </summary>
    public enum VetStatus
    {
        Pending = 0,   // Default status after registration
        Approved = 1,  // Can log in
        Rejected = 2   // Blocked from login
    }

    /// <summary>
    /// Veterinarian Model Class
    /// Represents doctors/vets in the system with profile & authentication details
    /// </summary>
    public class Veterinarian
    {
        /// <summary>
        /// Primary Key (Unique Vet ID)
        /// </summary>
        [Key]
        public int VetId { get; set; }

        /// <summary>
        /// Full Name of Veterinarian
        /// Only alphabets and spaces allowed
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name must contain only letters")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Email Address of Veterinarian (Unique login credential)
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        /// <summary>
        /// Contact Number of Veterinarian
        /// Only digits allowed (10–15 characters)
        /// </summary>
        [Required(ErrorMessage = "Contact Number is required")]
        [RegularExpression(@"^[0-9]{10,15}$", ErrorMessage = "Contact number must be 10 to 15 digits")]
        public string Contact { get; set; }

        /// <summary>
        /// Clinic/Practice Address
        /// </summary>
        [Required(ErrorMessage = "Address is required")]
        [StringLength(100, ErrorMessage = "Address cannot exceed 100 characters")]
        public string Address { get; set; }

        /// <summary>
        /// Password Hash (Encrypted password stored here)
        /// Plain passwords should NEVER be stored
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Area of specialization (optional)
        /// Example: Surgery, Vaccination, Dental, etc.
        /// </summary>
        [StringLength(100)]
        public string Specialization { get; set; }

        /// <summary>
        /// Years of Experience (0–50 allowed)
        /// </summary>
        [Range(0, 50, ErrorMessage = "Experience must be between 0 and 50 years")]
        public int? Experience { get; set; }

        /// <summary>
        /// Available time slots for appointments (e.g., Mon-Fri 2-5 PM)
        /// </summary>
        [StringLength(200)]
        public string AvailableSlots { get; set; }

        /// <summary>
        /// Account Status (Pending by default)
        /// Admin/Owner must approve before login
        /// </summary>
        public VetStatus Status { get; set; } = VetStatus.Pending;

        /// <summary>
        /// Date of Registration
        /// Auto-set at the time of creation
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

