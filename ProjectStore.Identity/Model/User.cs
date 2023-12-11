using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectStore.Identity.Model
{
    public class User
    {
        public int UserId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
