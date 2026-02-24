using MedicalEquipmentManagementSystem.Data;
using MedicalEquipmentManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalEquipmentManagementSystem.Controllers;

/// <summary>
/// Kontroler obsługujący operacje CRUD na sprzęcie medycznym.
/// Dostęp do wszystkich akcji wymaga zalogowania użytkownika.
/// </summary>
[Authorize]
public class MedicalEquipmentController : Controller
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Inicjalizuje now� instancj� kontrolera.
    /// </summary>
    /// <param name="context">Kontekst bazy danych.</param>
    public MedicalEquipmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Wy�wietla list� ca�ego sprz�tu medycznego.
    /// </summary>
    /// <returns>Widok z list� sprz�tu.</returns>
    public async Task<IActionResult> Index()
    {
        var equipment = await _context.MedicalEquipments
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return View(equipment);
    }

    /// <summary>
    /// Wy�wietla szczeg�y wybranego sprz�tu.
    /// </summary>
    /// <param name="id">Identyfikator sprz�tu.</param>
    /// <returns>Widok ze szczeg�ami sprz�tu lub NotFound.</returns>
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var equipment = await _context.MedicalEquipments
            .FirstOrDefaultAsync(e => e.Id == id);

        if (equipment is null)
        {
            return NotFound();
        }

        return View(equipment);
    }

    /// <summary>
    /// Wy�wietla formularz tworzenia nowego sprz�tu.
    /// </summary>
    /// <returns>Widok formularza tworzenia.</returns>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Przetwarza formularz tworzenia nowego sprz�tu.
    /// </summary>
    /// <param name="equipment">Dane nowego sprz�tu.</param>
    /// <returns>Przekierowanie do listy lub widok z b��dami walidacji.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,SerialNumber,Manufacturer,Model,Category,Location,Status,PurchaseDate,LastMaintenanceDate,NextMaintenanceDate,Notes")] MedicalEquipment equipment)
    {
        if (await _context.MedicalEquipments.AnyAsync(e => e.SerialNumber == equipment.SerialNumber))
        {
            ModelState.AddModelError(nameof(equipment.SerialNumber), "Sprz�t o tym numerze seryjnym ju� istnieje.");
        }

        if (ModelState.IsValid)
        {
            equipment.CreatedAt = DateTime.UtcNow;
            _context.Add(equipment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Pomy�lnie dodano sprz�t: {equipment.Name}";

            return RedirectToAction(nameof(Index));
        }

        return View(equipment);
    }

    /// <summary>
    /// Wy�wietla formularz edycji sprz�tu.
    /// </summary>
    /// <param name="id">Identyfikator sprz�tu do edycji.</param>
    /// <returns>Widok formularza edycji lub NotFound.</returns>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var equipment = await _context.MedicalEquipments.FindAsync(id);

        if (equipment is null)
        {
            return NotFound();
        }

        return View(equipment);
    }

    /// <summary>
    /// Przetwarza formularz edycji sprz�tu.
    /// </summary>
    /// <param name="id">Identyfikator sprz�tu.</param>
    /// <param name="equipment">Zaktualizowane dane sprz�tu.</param>
    /// <returns>Przekierowanie do listy lub widok z b��dami walidacji.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SerialNumber,Manufacturer,Model,Category,Location,Status,PurchaseDate,LastMaintenanceDate,NextMaintenanceDate,Notes")] MedicalEquipment equipment)
    {
        if (id != equipment.Id)
        {
            return NotFound();
        }

        if (await _context.MedicalEquipments.AnyAsync(e => e.SerialNumber == equipment.SerialNumber && e.Id != id))
        {
            ModelState.AddModelError(nameof(equipment.SerialNumber), "Sprz�t o tym numerze seryjnym ju� istnieje.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingEquipment = await _context.MedicalEquipments.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                if (existingEquipment is null)
                {
                    return NotFound();
                }

                equipment.CreatedAt = existingEquipment.CreatedAt;
                equipment.UpdatedAt = DateTime.UtcNow;
                _context.Update(equipment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Pomy�lnie zaktualizowano sprz�t: {equipment.Name}";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EquipmentExistsAsync(equipment.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(equipment);
    }

    /// <summary>
    /// Wy�wietla potwierdzenie usuni�cia sprz�tu.
    /// </summary>
    /// <param name="id">Identyfikator sprz�tu do usuni�cia.</param>
    /// <returns>Widok potwierdzenia usuni�cia lub NotFound.</returns>
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var equipment = await _context.MedicalEquipments
            .FirstOrDefaultAsync(e => e.Id == id);

        if (equipment is null)
        {
            return NotFound();
        }

        return View(equipment);
    }

    /// <summary>
    /// Przetwarza usuni�cie sprz�tu.
    /// </summary>
    /// <param name="id">Identyfikator sprz�tu do usuni�cia.</param>
    /// <returns>Przekierowanie do listy.</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var equipment = await _context.MedicalEquipments.FindAsync(id);

        if (equipment is not null)
        {
            var name = equipment.Name;
            _context.MedicalEquipments.Remove(equipment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Pomy�lnie usuni�to sprz�t: {name}";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Sprawdza czy sprz�t o podanym Id istnieje.
    /// </summary>
    /// <param name="id">Identyfikator sprz�tu.</param>
    /// <returns>True je�li sprz�t istnieje, w przeciwnym razie false.</returns>
    private async Task<bool> EquipmentExistsAsync(int id)
    {
        return await _context.MedicalEquipments.AnyAsync(e => e.Id == id);
    }
}
