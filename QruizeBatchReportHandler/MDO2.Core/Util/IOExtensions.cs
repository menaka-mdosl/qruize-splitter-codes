namespace MDO2.Core.Util
{
    public static class IOExtensions
    {
        public static string RemoveInvalidPathChar(this string path, string replaceWith = "")
        {
            var chars = System.IO.Path.GetInvalidPathChars();
            var replaced = path;
            foreach (var item in chars)
            {
                var str = new string(new char[] { item });
                replaced = replaced.Replace(str, replaceWith);
            }
            return replaced;
        }
        public static string RemoveInvalidFileNameChar(this string path, string replaceWith = "")
        {
            var chars = System.IO.Path.GetInvalidFileNameChars();
            var replaced = path;
            foreach (var item in chars)
            {
                var str = new string(new char[] { item });
                replaced = replaced.Replace(str, replaceWith);
            }
            return replaced;
        }
    }
}
