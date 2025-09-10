using Microsoft.AspNetCore.Mvc;
using PetCare.Models.Veterinarian;
using System.Text;
using System.Security.Cryptography;
using VetModel = PetCare.Models.Veterinarian.Veterinarian;

namespace PetCare.Controllers.Veterinarian
{
    public class AuthController : Controller
    {
        private readonly ApplicationContext _context;

        // Constructor (Dependency Injection for DB)
        public AuthController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: Register Page (Veterinarian)
        /// </summary>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// POST: Register Veterinarian
        /// Default status = Pending
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(VetModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingVet = _context.Veterinarians.FirstOrDefault(v => v.Email == model.Email);
                if (existingVet != null)
                {
                    TempData["Error"] = "Email already registered!";
                    return View(model);
                }

                // Hash password before saving
                model.PasswordHash = HashPassword(model.PasswordHash);

                // Default status is Pending
                model.Status = VetStatus.Pending;

                _context.Veterinarians.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "Registration successful! Wait for admin approval.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        /// <summary>
        /// GET: Login Page
        /// </summary>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// POST: Veterinarian Login
        /// Only approved vets can log in
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            var vet = _context.Veterinarians.FirstOrDefault(v => v.Email == email);

            if (vet == null || vet.PasswordHash != HashPassword(password))
            {
                TempData["Error"] = "Invalid credentials!";
                return View();
            }

            // Check vet status
            if (vet.Status == VetStatus.Pending)
            {
                TempData["SweetAlert"] = "pending";
                return View();
            }
            else if (vet.Status == VetStatus.Rejected)
            {
                TempData["SweetAlert"] = "rejected";
                return View();
            }

            // Approved -> Login success
            HttpContext.Session.SetInt32("VetId", vet.VetId);
            HttpContext.Session.SetString("VetName", vet.Name);

            return RedirectToAction("Dashboard", "Veterinarian");
        }

        /// <summary>
        /// Password Hashing using SHA256
        /// (Never store plain text passwords)
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}