using System;

namespace UP_Murtazin.Models
{
    public enum NotificationType
    {
        Critical,   // Критическая ошибка
        Warning,    // Предупреждение
        Info        // Информационное сообщение
    }

    public class Notification
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public int DisplayTime { get; set; } // Время показа в секундах

        public Notification()
        {
            Timestamp = DateTime.Now;
        }

        public Notification(string title, string message, NotificationType type) : this()
        {
            Title = title;
            Message = message;
            Type = type;

            // Устанавливаем время показа в зависимости от типа
            switch (type)
            {
                case NotificationType.Critical:
                    DisplayTime = 10;
                    break;
                case NotificationType.Warning:
                    DisplayTime = 7;
                    break;
                default:
                    DisplayTime = 5;
                    break;
            }
        }

        public string GetIcon()
        {
            switch (Type)
            {
                case NotificationType.Critical:
                    return "⚠️";
                case NotificationType.Warning:
                    return "⚠️";
                default:
                    return "ℹ️";
            }
        }

        public string GetBackgroundColor()
        {
            switch (Type)
            {
                case NotificationType.Critical:
                    return "#E74C3C";
                case NotificationType.Warning:
                    return "#F39C12";
                default:
                    return "#27AE60";
            }
        }
    }
}