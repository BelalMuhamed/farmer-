using CRDB_Farmers.DTos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRDB_Farmers.Helper
{
    internal static class Folder
    {
        public static string CreateFolder(string baseDirectoryPath, string folderSuffix)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string folderName = $"{folderSuffix}_{timestamp}";

            string fullFolderPath = Path.Combine(baseDirectoryPath, folderName);

            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            return fullFolderPath;
        }

    }
}
