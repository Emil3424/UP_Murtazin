using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using UP_Murtazin.Models;

namespace UP_Murtazin.Helpers
{
    public class NotificationManager
    {
        private static NotificationManager _instance;
        private Queue<Notification> notificationQueue = new Queue<Notification>();
        private bool isShowing = false;
        private Panel notificationPanel;
        private DispatcherTimer timer;
        private Notification currentNotification;
        private Border currentBorder;

        private NotificationManager() { }

        public static NotificationManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NotificationManager();
                return _instance;
            }
        }

        public void Initialize(Panel panel)
        {
            notificationPanel = panel;

            // Создаем таймер для автоматического закрытия
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
        }

        public void ShowNotification(string title, string message, NotificationType type)
        {
            var notification = new Notification(title, message, type);

            // Логируем уведомление
            LogNotification(notification);

            // Добавляем в очередь
            notificationQueue.Enqueue(notification);

            // Начинаем показ, если не показывается
            if (!isShowing)
            {
                ShowNextNotification();
            }
        }

        private void ShowNextNotification()
        {
            if (notificationQueue.Count == 0)
            {
                isShowing = false;
                return;
            }

            isShowing = true;
            currentNotification = notificationQueue.Dequeue();

            // Создаем и показываем уведомление
            ShowNotificationUI(currentNotification);
        }

        private void ShowNotificationUI(Notification notification)
        {
            if (notificationPanel == null) return;

            // Создаем границу уведомления
            currentBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(notification.GetBackgroundColor())),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 10),
                MaxWidth = 350,
                MinWidth = 300,
                Effect = new DropShadowEffect { BlurRadius = 8, ShadowDepth = 2, Opacity = 0.3 }
            };

            // Создаем основной стек
            var mainStack = new StackPanel();

            // Верхняя панель с заголовком и кнопкой закрытия
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titlePanel = new StackPanel { Orientation = Orientation.Horizontal };
            titlePanel.Children.Add(new TextBlock
            {
                Text = $"{notification.GetIcon()}  ",
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center
            });
            titlePanel.Children.Add(new TextBlock
            {
                Text = notification.Title,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            });

            var closeButton = new Button
            {
                Content = "✖",
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.White,
                FontSize = 12,
                Cursor = System.Windows.Input.Cursors.Hand,
                Width = 24,
                Height = 24,
                Tag = notification
            };
            closeButton.Click += CloseButton_Click;

            Grid.SetColumn(titlePanel, 0);
            Grid.SetColumn(closeButton, 1);
            headerGrid.Children.Add(titlePanel);
            headerGrid.Children.Add(closeButton);

            // Сообщение
            var messageText = new TextBlock
            {
                Text = notification.Message,
                FontSize = 12,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 0)
            };

            // Кнопка подтверждения для критических уведомлений
            if (notification.Type == NotificationType.Critical)
            {
                var confirmButton = new Button
                {
                    Content = "Понятно",
                    Background = new SolidColorBrush(Colors.White),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(notification.GetBackgroundColor())),
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(12, 5, 12, 5),
                    Margin = new Thickness(0, 8, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                confirmButton.Click += ConfirmButton_Click;

                mainStack.Children.Add(headerGrid);
                mainStack.Children.Add(messageText);
                mainStack.Children.Add(confirmButton);
            }
            else
            {
                mainStack.Children.Add(headerGrid);
                mainStack.Children.Add(messageText);
            }

            currentBorder.Child = new Border
            {
                Padding = new Thickness(12, 10, 12, 10),
                Child = mainStack
            };

            // Добавляем уведомление в панель
            notificationPanel.Children.Add(currentBorder);

            // Запускаем таймер на закрытие
            timer.Interval = TimeSpan.FromSeconds(currentNotification.DisplayTime);
            timer.Start();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                // Останавливаем таймер
                timer.Stop();

                // Удаляем уведомление
                if (currentBorder != null && notificationPanel.Children.Contains(currentBorder))
                {
                    notificationPanel.Children.Remove(currentBorder);
                }

                // Показываем следующее
                ShowNextNotification();
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Для критических уведомлений - просто закрываем
            timer.Stop();

            if (currentBorder != null && notificationPanel.Children.Contains(currentBorder))
            {
                notificationPanel.Children.Remove(currentBorder);
            }

            ShowNextNotification();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();

            if (currentBorder != null && notificationPanel.Children.Contains(currentBorder))
            {
                notificationPanel.Children.Remove(currentBorder);
            }

            ShowNextNotification();
        }

        private void LogNotification(Notification notification)
        {
            string logMessage = $"[{notification.Timestamp:yyyy-MM-dd HH:mm:ss}] [{notification.Type}] {notification.Title}: {notification.Message}";

            // Записываем в файл лога
            try
            {
                string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "notifications.log");
                System.IO.File.AppendAllText(logPath, logMessage + Environment.NewLine);
            }
            catch { }

            // Также выводим в отладку
            System.Diagnostics.Debug.WriteLine(logMessage);
        }
    }
}