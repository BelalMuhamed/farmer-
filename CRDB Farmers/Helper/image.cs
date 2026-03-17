using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;


namespace CRDB_Farmers.Helper
{
    internal static class image
    {
        //public static void ForceConvertBase64ToPng(string base64String, string filePath)
        //{
        //    if (base64String.Contains(","))
        //        base64String = base64String.Substring(base64String.IndexOf(",") + 1);

        //    byte[] imageBytes = Convert.FromBase64String(base64String);

        //     var ms = new MemoryStream(imageBytes);
        //     var img = System.Drawing.Image.FromStream(ms);
        //    img.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        //}
        //public static void ForceConvertBase64ToPng(string base64String, string directoryPath, string imageName)
        //{
        //    if (string.IsNullOrWhiteSpace(base64String))
        //        throw new ArgumentException("Base64 string is null or empty.");

        //    // If it's a data URI, strip the prefix
        //    if (base64String.Contains(","))
        //        base64String = base64String.Substring(base64String.IndexOf(",") + 1);

        //    // Clean whitespace and newlines
        //    base64String = base64String.Trim().Replace(" ", "").Replace("\r", "").Replace("\n", "");

        //    byte[] imageBytes;
        //    try
        //    {
        //        imageBytes = Convert.FromBase64String(base64String);
        //    }
        //    catch (FormatException ex)
        //    {
        //        throw new FormatException("The provided string is not valid Base64.", ex);
        //    }

        //     var ms = new MemoryStream(imageBytes);
        //     var img = System.Drawing.Image.FromStream(ms);

        //    Directory.CreateDirectory(directoryPath);

        //    string filePath = Path.Combine(directoryPath, imageName + ".png");

        //    img.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        //}

        //public static void ForceConvertBase64ToPng(string base64String, string directoryPath, string imageName)
        //{
        //    if (string.IsNullOrWhiteSpace(base64String))
        //        throw new ArgumentException("Base64 string is null or empty.");

        //    // Remove any metadata prefix (e.g. "data:image/jpeg;base64,")
        //    if (base64String.Contains(","))
        //        base64String = base64String.Substring(base64String.IndexOf(",") + 1);

        //    // Clean string
        //    base64String = base64String
        //        .Trim()
        //        .Replace(" ", "")
        //        .Replace("\r", "")
        //        .Replace("\n", "")
        //        .Replace("\"", "");

        //    // Fix padding
        //    int mod = base64String.Length % 4;
        //    if (mod > 0)
        //        base64String += new string('=', 4 - mod);

        //    byte[] imageBytes = Convert.FromBase64String(base64String);

        //     var ms = new MemoryStream(imageBytes);
        //     var img = System.Drawing.Image.FromStream(ms);

        //    Directory.CreateDirectory(directoryPath);

        //    string filePath = Path.Combine(directoryPath, imageName + ".png");

        //    img.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        //}

        public static void ForceConvertBase64ToPng(string base64String, string directoryPath, string imageName,ref string ErrorMessage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(base64String))
                {
                    ErrorMessage = $"image for{imageName} is null or empty ";
                    throw new ArgumentException("Base64 string is null or empty.");

                }

                // Remove prefix like: "data:image/jpeg;base64,"
                if (base64String.Contains(","))
                    base64String = base64String.Substring(base64String.IndexOf(",") + 1);

                // Clean and fix padding
                base64String = base64String
                    .Trim()
                    .Replace(" ", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Replace("\"", "");

                int mod = base64String.Length % 4;
                if (mod > 0)
                    base64String += new string('=', 4 - mod);

                byte[] imageBytes = Convert.FromBase64String(base64String);

                 var ms = new MemoryStream(imageBytes);

                // This line can throw if the stream is not a valid image
                 var img = System.Drawing.Image.FromStream(ms, true, true);

                Directory.CreateDirectory(directoryPath);
                string filePath = Path.Combine(directoryPath, imageName + ".png");

                img.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (FormatException fex)
            {
                ErrorMessage = $"Invalid base64 format: {fex.Message} in image for {imageName}";
                Console.WriteLine($"Invalid base64 format: {fex.Message} in image for {imageName}");
            }
            catch (ArgumentException aex)
            {
                ErrorMessage = $"Image conversion error: {aex.Message} in image for {imageName}";
                Console.WriteLine($"Image conversion error: {aex.Message} in image for {imageName}");
            }
            catch (OutOfMemoryException omex)
            {
                ErrorMessage = $"Image data may be corrupt or not an image. OutOfMemoryException: {omex.Message} in image for {imageName}";
                Console.WriteLine($"Image data may be corrupt or not an image. OutOfMemoryException: {omex.Message} in image for {imageName}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected error: {ex.Message} in image for {imageName}";
                Console.WriteLine($"Unexpected error: {ex.Message} in image for {imageName}");
            }
        }


    }
}
