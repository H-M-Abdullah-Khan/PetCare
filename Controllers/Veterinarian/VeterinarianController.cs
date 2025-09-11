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

                    TempData["success"] = "Login successful!";
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
            if (HttpContext.Session.GetInt32("VetId") == null)
            {
                return RedirectToAction(nameof(Login));
            }
            ViewBag.VetCount = _context.Veterinarians.Count();
            return View();
        }

        // Details of specific vet by id
        public IActionResult Details(int? id)
        {
            if (HttpContext.Session.GetInt32("VetId") == null)
                return RedirectToAction(nameof(Login));
            if (id == null)
                return NotFound();

            var vet = _context.Veterinarians.FirstOrDefault(m => m.VetId == id);
            if (vet == null)
                return NotFound();

            return View(vet);
        }

        // Delete confirmation page
        public IActionResult Delete(int? id)
        {
            if (HttpContext.Session.GetInt32("VetId") == null)
                return RedirectToAction(nameof(Login));
            if (id == null)
                return NotFound();

            var vet = _context.Veterinarians.FirstOrDefault(m => m.VetId == id);
            if (vet == null)
                return NotFound();

            return View(vet);
        }

        // POST: Delete confirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetInt32("VetId") == null)
                return RedirectToAction(nameof(Login));

            var vet = await _context.Veterinarians.FindAsync(id);
            if (vet != null)
            {
                _context.Veterinarians.Remove(vet);
                await _context.SaveChangesAsync();

                TempData["success"] = "Veterinarian deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
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