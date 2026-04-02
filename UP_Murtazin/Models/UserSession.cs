namespace UP_Murtazin.Models
{
    public class UserSession
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Phone { get; set; }
        public bool IsManager { get; set; }
        public bool IsEngineer { get; set; }
        public string ImageBase64 { get; set; }

        // Свойства для отображения
        public string ShortName
        {
            get
            {
                if (string.IsNullOrEmpty(FullName)) return Email;
                var parts = FullName.Split(' ');
                if (parts.Length >= 2)
                {
                    return $"{parts[0]} {parts[1][0]}.{parts[2][0]}.";
                }
                return FullName;
            }
        }

        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(FullName)) return "?";
                var parts = FullName.Split(' ');
                if (parts.Length >= 2)
                {
                    return $"{parts[0][0]}{parts[1][0]}";
                }
                return FullName.Substring(0, 1);
            }
        }

        public string RoleName
        {
            get
            {
                if (IsManager && IsEngineer) return "Старший инженер";
                if (IsManager) return "Менеджер";
                if (IsEngineer) return "Инженер";
                return Role ?? "Сотрудник";
            }
        }
    }
}