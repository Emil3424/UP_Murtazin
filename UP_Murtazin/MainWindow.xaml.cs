using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP_Murtazin.Helpers;
using UP_Murtazin.Models;
using UP_Murtazin.Okna;
using UP_Murtazin.Pages;

namespace UP_Murtazin
{
    public partial class MainWindow : Window
    {
        private UserSession _currentUser;

        // Конструктор без параметров
        public MainWindow(UserSession user)
        {
            InitializeComponent();

            NotificationManager.Instance.Initialize(NotificationPanel);

            _currentUser = user;
            SetupUIForUser();
            MainFrame.Navigate(new HomePage());

        }
        private void SetupUIForUser()
        {
            if (_currentUser == null) return;

            // Обновляем отображение имени пользователя в верхней панели
            UserNameText.Text = _currentUser.ShortName;
            UserRoleText.Text = _currentUser.RoleName;

            // Обновляем Popup меню
            PopupUserNameText.Text = _currentUser.FullName;
            PopupUserRoleText.Text = _currentUser.RoleName;
            PopupUserEmailText.Text = _currentUser.Email;

            if (!string.IsNullOrEmpty(_currentUser.ImageBase64))
            {
                UserAvatar.Source = ImageHelper.ConvertBase64ToImage(_currentUser.ImageBase64);
                UserAvatar1.Source = ImageHelper.ConvertBase64ToImage(_currentUser.ImageBase64);
            }
            else
            {
                // Если нет аватарки, показываем заглушку с инициалами
                UserAvatar.Visibility = Visibility.Collapsed;
                UserAvatar1.Visibility = Visibility.Collapsed;
            }

            // Скрываем пункты меню для обычных пользователей
            if (!_currentUser.IsManager && !_currentUser.IsEngineer)
            {
                ToggleAdmin.Visibility = Visibility.Collapsed;
                BtnDetailedReports.Visibility = Visibility.Collapsed;
                BtnInventory.Visibility = Visibility.Collapsed;
            }
        }
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = !ProfilePopup.IsOpen;
        }

        private void ViewProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = false;
            // Создаем и открываем окно профиля
            var profileWindow = new ProfileWindow(_currentUser);
            profileWindow.Owner = this;
            profileWindow.ShowDialog();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = false;
            // Создаем и открываем окно настроек
            var settingsWindow = new SettingsWindow(_currentUser);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = false;

            var result = MessageBox.Show("Вы уверены, что хотите выйти из системы?",
                "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Запускаем новое приложение
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);

                // Закрываем текущее приложение
                Application.Current.Shutdown();
            }
        }

        // Навигационные методы
        private void BtnMain_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HomePage());
        }

        private void BtnMonitor_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MonitorPage());
        }

        private void BtnDetailedReports_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new DetailedReportsPage());
        }

        private void BtnInventory_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new InventoryPage());
        }

        private void BtnMachines_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MachinesPage());
        }


        private void BtnCompanies_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CompaniesPage());
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UsersPage());
        }

        private void BtnModems_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ModemsPage());
        }

        private void BtnAdditional_Click(object sender, RoutedEventArgs e)
        {
            //MainFrame.Navigate(new AdditionalPage());
        }

        private void ToggleAdmin_Checked(object sender, RoutedEventArgs e)
        {
            AdminSubMenu.Visibility = Visibility.Visible;
        }

        private void ToggleAdmin_Unchecked(object sender, RoutedEventArgs e)
        {
            AdminSubMenu.Visibility = Visibility.Collapsed;
        }
    }
}