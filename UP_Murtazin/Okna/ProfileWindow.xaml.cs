using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using UP_Murtazin.Models;

namespace UP_Murtazin.Okna
{
    public partial class ProfileWindow : Window
    {
        public ProfileWindow(UserSession user)
        {
            InitializeComponent();

            if (user != null)
            {
                FullNameText.Text = user.FullName;
                EmailText.Text = user.Email;
                RoleText.Text = user.RoleName;
                PhoneText.Text = user.Phone ?? "Не указан";
                PositionText.Text = user.RoleName;
            }

            if (!string.IsNullOrEmpty(user.ImageBase64))
            {
                UserAvatar.Source = ConvertBase64ToImage(user.ImageBase64);
            }
            else
            {
                // Если нет аватарки, показываем заглушку с инициалами
                UserAvatar.Visibility = Visibility.Collapsed;
            }
        }
        private BitmapImage ConvertBase64ToImage(string base64String)
        {
            try
            {
                // Убираем префикс "data:image/png;base64," если он есть
                string base64Data = base64String;
                if (base64String.Contains(","))
                {
                    base64Data = base64String.Substring(base64String.IndexOf(",") + 1);
                }

                byte[] imageBytes = Convert.FromBase64String(base64Data);

                using (var stream = new MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка конвертации изображения: {ex.Message}");
                return null;
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}