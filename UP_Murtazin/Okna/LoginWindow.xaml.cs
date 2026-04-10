using System;
using System.Data;
using System.Linq;
using System.Windows;
using UP_Murtazin.DB;
using UP_Murtazin.Models;
using UP_Murtazin.Services;

namespace UP_Murtazin.Okna
{
    public partial class LoginWindow : Window
    {
        private UP_MurtazinEntities dbContext;
        private CaptchaData currentCaptcha;
        public UserSession CurrentUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            dbContext = new UP_MurtazinEntities();
            GenerateCaptcha();

            // Убираем установку Owner - это не нужно для ShowDialog()
            // Owner будет установлен при вызове ShowDialog()
        }

        private void GenerateCaptcha()
        {
            Random random = new Random();

            // Генерируем случайные числа
            int x = random.Next(1, 20);
            int y = random.Next(1, 20);
            int c = random.Next(1, 10);
            int p = random.Next(1, 5);

            // Случайно выбираем операторы для 3-х действий (без деления)
            string[] operators = { "+", "-", "*" };
            string op1 = operators[random.Next(0, 3)];
            string op2 = operators[random.Next(0, 3)];
            string op3 = operators[random.Next(0, 3)];

            // Формируем выражение с 3-мя действиями
            string expression;
            double result;
            var table = new DataTable();

            // Используем DataTable.Compute для вычисления
            expression = $"{x} {op1} {y} {op2} {c} {op3} {p}";
            result = Convert.ToDouble(table.Compute(expression, null));

            currentCaptcha = new CaptchaData
            {
                Expression = expression,
                Result = Math.Round(result, 2)
            };

            CaptchaText.Text = expression + " = ?";
        }

        private void RefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
            CaptchaAnswerTextBox.Text = "";
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string captchaAnswer = CaptchaAnswerTextBox.Text.Trim();

            // Проверка заполнения
            if (string.IsNullOrEmpty(email))
            {
                ShowError("Введите email");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Введите пароль");
                return;
            }

            if (string.IsNullOrEmpty(captchaAnswer))
            {
                ShowError("Введите ответ капчи");
                return;
            }

            // Проверка капчи
            if (!double.TryParse(captchaAnswer, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double userAnswer))
            {
                ShowError("Неверный формат ответа капчи");
                GenerateCaptcha();
                return;
            }

            if (Math.Abs(userAnswer - currentCaptcha.Result) > 0.01)
            {
                ShowError($"Неверный ответ капчи. Правильный ответ: {currentCaptcha.Result}");
                GenerateCaptcha();
                CaptchaAnswerTextBox.Text = "";
                return;
            }

            ErrorText.Visibility = Visibility.Collapsed;

            try
            {
                // Поиск пользователя в БД
                var user = dbContext.users.FirstOrDefault(u => u.email == email);

                if (user == null)
                {
                    ShowError("Пользователь с таким email не найден");
                    return;
                }

                // Проверка пароля
                bool isPasswordValid = false;

                // Если пароль не установлен, устанавливаем его
                if (string.IsNullOrEmpty(user.passsword))
                {
                    // Устанавливаем пароль "123" для пользователя
                    string defaultPassword = "123";
                    user.passsword = PasswordHasher.HashPassword(defaultPassword);
                    dbContext.SaveChanges();
                }

                isPasswordValid = PasswordHasher.VerifyPassword(password, user.passsword);

                if (!isPasswordValid)
                {
                    ShowError("Неверный пароль");
                    return;
                }

                // Создаем сессию пользователя
                CurrentUser = new UserSession
                {
                    UserId = user.user_id,
                    Email = user.email,
                    FullName = user.full_name,
                    Phone = user.phone,
                    Role = user.role,
                    IsManager = user.is_manager == "true",
                    IsEngineer = user.is_engineer == "true",
                    ImageBase64 = user.image
                };

                var mainWindow = new MainWindow(CurrentUser); // замените на имя вашего главного окна
                mainWindow.Show();

                // Просто закрываем окно, DialogResult не нужен
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка входа: {ex.Message}");
                LoginButton.IsEnabled = true;
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
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

    public class CaptchaData
    {
        public string Expression { get; set; }
        public double Result { get; set; }
    }
}