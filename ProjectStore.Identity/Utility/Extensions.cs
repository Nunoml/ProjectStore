using Microsoft.CodeAnalysis.CSharp.Syntax;
using ProjectStore.Identity.RequestObject;
using ProjectStore.Identity.Model;

namespace ProjectStore.Identity.Utility
{
    public static class Extensions
    {
        public static User AsUser(this RegisterUserRequest request)
        {
            User user = new User();
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.Password = request.Password;

            return user;
        }
    }
}
