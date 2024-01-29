namespace CrmCorner.Models.PasswordHash
{
    public class SecureKey
    {
        public static string getKey()
        {
            //Random rnd = new Random();
            //int salt = rnd.Next(100000000,999999999);
            var key = "@Pass!2024";
            return key.ToString();
        }
    }
}
