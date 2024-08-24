namespace StoreModels.Dtos
{
    public class RegistrationRequestDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? StreetAddress { get; set; }
        public int? CompanyId { get; set; } = 0;

        public string? Role { get; set; }
    }
}
