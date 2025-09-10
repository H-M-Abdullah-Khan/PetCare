using Microsoft.AspNetCore.Mvc;

namespace PetCare.Controllers.Veterinarian
{
    public class VeterinarianController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
