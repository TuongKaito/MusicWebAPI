
namespace OnlineMusicServices.API.Utility
{
    using BCrypt.Net;

    public class HashingPassword
    {
        private static string GetRandomSalt()
        {
            return BCrypt.GenerateSalt(13);
        }

        public static string HashPassword(string password)
        {
            return BCrypt.HashPassword(password, GetRandomSalt());
        }

        public static bool ValidatePassword(string password, string correctHash)
        {
            return BCrypt.Verify(password, correctHash);
        }
    }
}