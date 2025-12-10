using System.ComponentModel.DataAnnotations;

namespace MedicalEquipmentManagementSystem.Models;

/// <summary>
/// Reprezentuje sprzêt medyczny w systemie zarz¹dzania.
/// </summary>
public class MedicalEquipment
{
    /// <summary>
    /// Unikalny identyfikator sprzêtu.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nazwa sprzêtu medycznego.
    /// </summary>
    [Required(ErrorMessage = "Nazwa jest wymagana")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Nazwa musi mieæ od 2 do 200 znaków")]
    [Display(Name = "Nazwa")]
    public required string Name { get; set; }

    /// <summary>
    /// Numer seryjny sprzêtu.
    /// </summary>
    [Required(ErrorMessage = "Numer seryjny jest wymagany")]
    [StringLength(100, ErrorMessage = "Numer seryjny mo¿e mieæ maksymalnie 100 znaków")]
    [Display(Name = "Numer seryjny")]
    public required string SerialNumber { get; set; }

    /// <summary>
    /// Producent sprzêtu.
    /// </summary>
    [StringLength(150, ErrorMessage = "Nazwa producenta mo¿e mieæ maksymalnie 150 znaków")]
    [Display(Name = "Producent")]
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Model sprzêtu.
    /// </summary>
    [StringLength(100, ErrorMessage = "Model mo¿e mieæ maksymalnie 100 znaków")]
    [Display(Name = "Model")]
    public string? Model { get; set; }

    /// <summary>
    /// Kategoria sprzêtu (np. diagnostyczny, terapeutyczny, laboratoryjny).
    /// </summary>
    [Required(ErrorMessage = "Kategoria jest wymagana")]
    [StringLength(100, ErrorMessage = "Kategoria mo¿e mieæ maksymalnie 100 znaków")]
    [Display(Name = "Kategoria")]
    public required string Category { get; set; }

    /// <summary>
    /// Lokalizacja sprzêtu w placówce.
    /// </summary>
    [StringLength(200, ErrorMessage = "Lokalizacja mo¿e mieæ maksymalnie 200 znaków")]
    [Display(Name = "Lokalizacja")]
    public string? Location { get; set; }

    /// <summary>
    /// Status sprzêtu (np. aktywny, w naprawie, wycofany).
    /// </summary>
    [Required(ErrorMessage = "Status jest wymagany")]
    [Display(Name = "Status")]
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Active;

    /// <summary>
    /// Data zakupu sprzêtu.
    /// </summary>
    [DataType(DataType.Date)]
    [Display(Name = "Data zakupu")]
    public DateOnly? PurchaseDate { get; set; }

    /// <summary>
    /// Data ostatniego przegl¹du.
    /// </summary>
    [DataType(DataType.Date)]
    [Display(Name = "Ostatni przegl¹d")]
    public DateOnly? LastMaintenanceDate { get; set; }

    /// <summary>
    /// Data nastêpnego planowanego przegl¹du.
    /// </summary>
    [DataType(DataType.Date)]
    [Display(Name = "Nastêpny przegl¹d")]
    public DateOnly? NextMaintenanceDate { get; set; }

    /// <summary>
    /// Dodatkowe uwagi dotycz¹ce sprzêtu.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Uwagi mog¹ mieæ maksymalnie 1000 znaków")]
    [Display(Name = "Uwagi")]
    public string? Notes { get; set; }

    /// <summary>
    /// Data utworzenia rekordu.
    /// </summary>
    [Display(Name = "Data utworzenia")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data ostatniej modyfikacji rekordu.
    /// </summary>
    [Display(Name = "Data modyfikacji")]
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Status sprzêtu medycznego.
/// </summary>
public enum EquipmentStatus
{
    [Display(Name = "Aktywny")]
    Active = 0,

    [Display(Name = "W naprawie")]
    UnderRepair = 1,

    [Display(Name = "Wycofany")]
    Decommissioned = 2,

    [Display(Name = "W konserwacji")]
    UnderMaintenance = 3,

    [Display(Name = "Oczekuje na dostawê")]
    PendingDelivery = 4
}
