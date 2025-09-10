using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PetCare.Models;

namespace PetCare.Controllers;

public class HomeController : Controller
{
   
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult About()
    {
        return View();
    }
    public IActionResult Services()
    {
        return View();
    }
    public IActionResult Shop()
    {
        return View();
    }
    public IActionResult AddtoCart()
    {
        return View();
    }
     public IActionResult Gallery()
    {
        return View();
    }
    public IActionResult AdoptPets()
    {
        return View();
    }
    public IActionResult Contact()
    {
        return View();
    }
    public IActionResult ProductDetail()
    {
        return View();
    }
}
