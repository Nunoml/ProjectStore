namespace ProjectStore.Identity.RequestObject
{
    public record EditUserRequest
    {
        public required string PasswordValidation;
        public string? Email;
        public string? NewPassword;
        public string? PhoneNumber;
        public string? FirstName;
        public string? LastName;
    }
}
