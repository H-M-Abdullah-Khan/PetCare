using Microsoft.AspNetCore.Mvc;
using PetCare.Models.Veterinarian;
using System.Text;
using System.Security.Cryptography;
using VetModel = PetCare.Models.Veterinarian.Veterinarian;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;

namespace PetCare.Controllers.Veterinarian
{
    public class VeterinarianController : Controller
    {
        private readonly ApplicationContext _context;

        public VeterinarianController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: Register page
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register new vet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(VetModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingVet = _context.Veterinarians.FirstOrDefault(v => v.Email.ToLower() == model.Email.ToLower());
            if (existingVet != null)
            {
                TempData["Error"] = "Email already registered!";
                return View(model);
            }

            model.PasswordHash = HashPassword(model.Password);
            model.Status = VetStatus.Pending;

            model.Password = null; // Clear plain password

            _context.Veterinarians.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Registration successful! Wait for admin approval.";
            return RedirectToAction("Login");
        }

        // GET: Login page
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            try
            {
                var hashedPassword = HashPassword(password);

                var storedVet = _context.Veterinarians
                    .FirstOrDefault(v => v.Email.ToLower() == email.ToLower());

                if (storedVet != null)
                {
                    // For debugging: show both hashes
                    ViewBag.DebugInfo = $"Input Hash: {hashedPassword}, Stored Hash: {storedVet.PasswordHash}";

                    if (storedVet.PasswordHash != hashedPassword)
                    {
                        ViewBag.Error = "Invalid password. Please check your password.";
                        return View();
                    }

                    // Check status
                    if (storedVet.Status == VetStatus.Pending)
                    {
                        TempData["SweetAlert"] = "pending";
                        ViewBag.Error = "Your account is pending approval by the admin.";
                        return View();
                    }
                    else if (storedVet.Status == VetStatus.Rejected)
                    {
                        TempData["SweetAlert"] = "rejected";
                        ViewBag.Error = "Your account has been rejected.";
                        return View();
                    }

                    // Approved vet, set session
                    HttpContext.Session.SetInt32("VetId", storedVet.VetId);
                    HttpContext.Session.SetString("VetName", storedVet.Name ?? string.Empty);

                    TempData["LoginSuccess"] = "Signed in successfully";
                    return RedirectToAction(nameof(Dashboard));
                }
                else
                {
                    ViewBag.Error = "Email not found. Please check your email address.";
                    return View();
                }
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = $"Login error: {ex.Message}";
                return View();
            }
        }

        // Logout clears session and redirects to login
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // Dashboard page for logged-in vets
        public IActionResult Dashboard()
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var vet = _context.Veterinarians.FirstOrDefault(v => v.VetId == vetId.Value);
            if (vet == null)
            {
                return RedirectToAction(nameof(Login));
            }

            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");

            return View();
        }

        public IActionResult Profile()
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null) return RedirectToAction(nameof(Login));

            var vet = _context.Veterinarians.FirstOrDefault(v => v.VetId == vetId.Value);
            if (vet == null) return RedirectToAction(nameof(Login));

            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
            return View(vet);
        }

        // GET: EditProfile
        public IActionResult EditProfile()
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null)
                return RedirectToAction("Login", "Veterinarian");

            var vet = _context.Veterinarians.FirstOrDefault(v => v.VetId == vetId.Value);
            if (vet == null)
                return RedirectToAction("Login", "Veterinarian");

            // For security, clear password hash before sending to view (optional)
            vet.PasswordHash = null;

            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
            return View(vet);
        }

        // POST: EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(VetModel model)
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null)
                return RedirectToAction("Login", "Veterinarian");

            var emailExists = _context.Veterinarians
                .Any(v => v.Email == model.Email && v.VetId != vetId.Value);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already in use.");
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            var vet = _context.Veterinarians.FirstOrDefault(v => v.VetId == vetId.Value);
            if (vet == null)
                return RedirectToAction("Login", "Veterinarian");

            vet.Name = model.Name;
            vet.Email = model.Email;
            vet.Contact = model.Contact;
            vet.Address = model.Address;
            vet.Specialization = model.Specialization;
            vet.Experience = model.Experience;
            vet.AvailableSlots = model.AvailableSlots;
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                vet.PasswordHash = model.Password; // yahan hashing lagani hogi
            }

            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
            TempData["Success"] = "Profile updated successfully!";

            _context.SaveChanges();

            return RedirectToAction("EditProfile");
        }

        // ChangePassword GET
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetInt32("VetId") == null) return RedirectToAction(nameof(Login));
            return View();
        }

        // ChangePassword POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null) return RedirectToAction(nameof(Login));

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "New password and confirmation do not match.";
                return View();
            }

            var vet = _context.Veterinarians.FirstOrDefault(v => v.VetId == vetId.Value);
            if (vet == null) return RedirectToAction(nameof(Login));

            // Check current password
            if (vet.PasswordHash != HashPassword(currentPassword))
            {
                ViewBag.Error = "Current password is incorrect.";
                return View();
            }

            // Update password
            vet.PasswordHash = HashPassword(newPassword);

            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
            TempData["success"] = "Password changed successfully!";

            _context.SaveChanges();
            return RedirectToAction(nameof(Profile));
        }

        // Appointments - all
        public IActionResult Appointments()
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null) return RedirectToAction(nameof(Login));

            // Fetch appointments - implement your own model & fetching logic
            var appointments = _context.Appointments.Where(a => a.VetId == vetId.Value).ToList();

            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
            return View(appointments);
        }

        // Upcoming appointments
        public IActionResult Upcoming()
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null) return RedirectToAction(nameof(Login));

            var upcoming = _context.Appointments
                .Where(a => a.VetId == vetId.Value && a.Date >= DateTime.Now && a.Status == AppointmentStatus.Scheduled)
                .OrderBy(a => a.Date)
                .ToList();

            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
            return View(upcoming);
        }

        // Completed appointments
        public IActionResult Completed()
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null) return RedirectToAction(nameof(Login));

            var completed = _context.Appointments
                .Where(a => a.VetId == vetId.Value && a.Status == AppointmentStatus.Completed)
                .OrderByDescending(a => a.Date)
                .ToList();
            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
            return View(completed);
        }

        // Cancelled appointments
        public IActionResult Cancelled()
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null) return RedirectToAction(nameof(Login));

            var cancelled = _context.Appointments
                .Where(a => a.VetId == vetId.Value && a.Status == AppointmentStatus.Cancelled)
                .OrderByDescending(a => a.Date)
                .ToList();
            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
            return View(cancelled);
        }

        // Pet Records
        public IActionResult PetRecords()
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null) return RedirectToAction(nameof(Login));

            // Fetch pet records linked to vet's patients
            var pets = _context.PetRecords
                .Where(p => p.VetId == vetId.Value)
                .ToList();
            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
            return View(pets);
        }

        // Schedule (Manage)
        public IActionResult Manage()
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null) return RedirectToAction(nameof(Login));

            // Your logic to fetch/manage availability schedule
            var schedule = _context.Schedules.Where(s => s.VetId == vetId.Value).ToList();
            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");
            return View(schedule);
        }
        
        // Inbox / Messages
        public IActionResult Inbox()
        {
            int? vetId = HttpContext.Session.GetInt32("VetId");
            if (vetId == null) return RedirectToAction(nameof(Login));

            // Fetch messages/notifications for this vet
            var messages = _context.Messages
                .Where(m => m.RecipientVetId == vetId.Value)
                .OrderByDescending(m => m.DateSent)
                .ToList(); 
            ViewBag.VetName = vet.Name ?? "Doctor";
            ViewBag.Specialization = string.IsNullOrEmpty(vet.Specialization) ? "Specialist" : vet.Specialization;
            ViewBag.CurrentDateTime = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");

            return View(messages);
        }

        // Helper: Hash password with SHA256
        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToHexString(hashedBytes).ToLower();
            }
        }
    }
}