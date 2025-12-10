using System.Diagnostics;
using MedicalEquipmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using MedicalEquipmentManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalEquipmentManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DbStatus()
        {
            try
            {
                var canConnect = await _db.Database.CanConnectAsync();
                var provider = _db.Database.ProviderName;
                var dbName = _db.Database.GetDbConnection().Database;
                return Ok(new { status = canConnect ? "Healthy" : "Unhealthy", provider, database = dbName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Unhealthy", error = ex.Message });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            //small test change
        }
    }
}
