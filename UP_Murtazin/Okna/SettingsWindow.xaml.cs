using System;
using System.Linq;
using System.Windows;
using UP_Murtazin.DB;
using UP_Murtazin.Models;
using UP_Murtazin.Services;

namespace UP_Murtazin.Okna
{
    public partial class SettingsWindow : Window
    {
        private UP_MurtazinEntities dbContext;
        private UserSession _currentUser;

        public SettingsWindow(UserSession user)
        {
            InitializeComponent();
            _currentUser = user;
            dbContext = new UP_MurtazinEntities();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(currentPassword))
            {
                ShowMessage("Введите текущий пароль", false);
                return;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ShowMessage("Введите новый пароль", false);
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowMessage("Новый пароль и подтверждение не совпадают", false);
                return;
            }

            if (newPassword.Length < 4)
            {
                ShowMessage("Пароль должен содержать минимум 4 символа", false);
                return;
            }

            try
            {
                var user = dbContext.users.FirstOrDefault(u => u.user_id == _currentUser.UserId);

                if (user == null)
                {
                    ShowMessage("Пользователь не найден", false);
                    return;
                }

                // Проверка текущего пароля
                bool isValid = false;
                if (string.IsNullOrEmpty(user.passsword))
                {
                    isValid = currentPassword == "123";
                }
                else
                {
                    isValid = PasswordHasher.VerifyPassword(currentPassword, user.passsword);
                }

                if (!isValid)
                {
                    ShowMessage("Неверный текущий пароль", false);
                    return;
                }

                // Хешируем новый пароль
                user.passsword = PasswordHasher.HashPassword(newPassword);
                dbContext.SaveChanges();

                ShowMessage("Пароль успешно изменен!", true);

                // Очищаем поля
                CurrentPasswordBox.Password = "";
                NewPasswordBox.Password = "";
                ConfirmPasswordBox.Password = "";
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка: {ex.Message}", false);
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            MessageText.Text = message;
            MessageText.Foreground = isSuccess ?
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green) :
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            MessageText.Visibility = Visibility.Visible;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (dbContext != null)
            {
                dbContext.Dispose();
            }
        }
    }
}