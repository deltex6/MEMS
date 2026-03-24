namespace MedicalEquipmentManagementSystem.Data;

/// <summary>
/// Zawiera zdefiniowane role systemowe w aplikacji.
/// </summary>
public static class UserRoles
{
    /// <summary>
    /// Administrator - zarządza systemem, persolenelem, serwisantami, lokalizacjami i sprzętem.
    /// </summary>
    public const string Administrator = "Administrator";

    /// <summary>
    /// Serwisant sprzętu - użytkownik odpowiedzialny za sprzęt i jego naprawy.
    /// </summary>
    public const string Technician = "Technician";

    /// <summary>
    /// Personel - użytkownik korzystający z danego sprzętu w celach medycznych.
    /// </summary>
    public const string Staff = "Staff";
}
