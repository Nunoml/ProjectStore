namespace ProjectStore.Identity.RequestObject
{
    public record EditUserRequest
    {
        public string PasswordValidation;
        public string Email;
        public string NewPassword;
        public string PhoneNumber;
        public string FirstName;
        public string LastName;
    }
}
