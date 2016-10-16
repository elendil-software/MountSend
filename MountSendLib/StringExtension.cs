namespace MountSend
{
    public static class StringExtension
    {
        public static string Left(this string value, uint length)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            
            return value.Length <= length ? value : value.Substring(0, (int)length);
        }
    }
}
