using System;
using System.IO;
using System.Linq;

namespace UP_Murtazin.Helpers
{
    public static class RandomSettingsImageHelper
    {
        private static readonly Random _random = new Random();
        private static readonly string[] _imageExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };

        /// <summary>
        /// Возвращает Pack URI к случайному изображению из папки /Resources/Settings/[subFolder]
        /// </summary>
        public static string GetRandomImagePath(string subFolder = "")
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(basePath, "Resources", "Settings", subFolder);

            if (!Directory.Exists(folderPath))
            {
                System.Diagnostics.Debug.WriteLine($"[RandomSettingsImage] Папка не найдена: {folderPath}");
                return "/Resources/Settings/default.png";
            }

            var files = Directory.GetFiles(folderPath)
                .Where(f => _imageExtensions.Contains(Path.GetExtension(f).ToLower()))
                .ToList();

            if (files.Count == 0)
                return "/Resources/Settings/default.png";

            string randomFile = files[_random.Next(files.Count)];
            string fileName = Path.GetFileName(randomFile);

            string subPath = string.IsNullOrEmpty(subFolder) ? "" : $"{subFolder}/";
            return $"pack://application:,,,/UP_Murtazin;component/Resources/Settings/{subPath}{fileName}";
        }
    }
}