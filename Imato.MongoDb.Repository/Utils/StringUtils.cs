namespace Imato.MongoDb.Repository
{
    public static class StringUtils
    {
        private static char[] characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890".ToCharArray();

        public static string NewId(ushort length = 5)
        {
            if (length <= 0 || length > 36)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            var result = string.Empty;
            for (int i = 0; i < length; i++)
            {
                int index = new Random().Next(0, characters.Length);
                result += characters[index];
            }

            return result;
        }
    }
}