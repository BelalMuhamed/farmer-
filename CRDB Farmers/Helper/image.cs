using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;


namespace CRDB_Farmers.Helper
{
    internal static class image
    {
        public static bool ForceConvertBase64ToPng(
      string base64String,
      string directoryPath,
      string imageName,
      out string error)
        {
            error = null;

            try
            {
                if (string.IsNullOrWhiteSpace(base64String))
                {
                    error = $"Image for {imageName} is null or empty";
                    return false;
                }

                if (base64String.Contains(","))
                    base64String = base64String.Substring(base64String.IndexOf(",") + 1);

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

                Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(directoryPath, $"{imageName}.png");

                using (var ms = new MemoryStream(imageBytes))
                using (var img = System.Drawing.Image.FromStream(ms, true, true))
                {
                    img.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                }

                // ✅ تحقق بعد الحفظ (هنا صح)
                if (!File.Exists(filePath))
                {
                    error = $"File was not created for {imageName}";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Image error for {imageName}: {ex.Message}";
                return false;
            }
        }


    }
}
