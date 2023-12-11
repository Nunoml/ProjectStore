using Microsoft.CodeAnalysis.CSharp.Syntax;
using ProjectStore.Identity.RequestObject;
using ProjectStore.Identity.Model;

namespace ProjectStore.Identity.Utility
{
    public static class Extensions
    {
        public static User AsUser(this RegisterUserRequest request)
        {
            User user = new User{
                FirstName = request.FirstName, LastName = request.LastName, Email = request.Email, Password = request.Password
            };
            user.PhoneNumber = request.PhoneNumber;
            return user;
        }
    }
}
