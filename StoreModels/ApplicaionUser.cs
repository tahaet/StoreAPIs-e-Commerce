using Microsoft.AspNetCore.Identity;

namespace StoreModels;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    public string Role { get; set; }
}
