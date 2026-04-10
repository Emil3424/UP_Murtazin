using System.Windows;
using UP_Murtazin.Helpers;
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
                UserAvatar.Source = ImageHelper.ConvertBase64ToImage(user.ImageBase64);
            }
            else
            {
                // Если нет аватарки, показываем заглушку с инициалами
                UserAvatar.Visibility = Visibility.Collapsed;
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}