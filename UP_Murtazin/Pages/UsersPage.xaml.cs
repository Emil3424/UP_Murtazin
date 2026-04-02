using System.Linq;
using System.Windows.Controls;
using UP_Murtazin.DB;

namespace UP_Murtazin.Pages
{
    public partial class UsersPage : Page
    {
        private UP_MurtazinEntities dbContext;

        public UsersPage()
        {
            InitializeComponent();
            dbContext = new UP_MurtazinEntities();
            LoadData();
        }

        private void LoadData()
        {
            var users = dbContext.users
                .Select(u => new
                {
                    UserId = u.user_id,
                    FullName = u.full_name,
                    Email = u.email,
                    Phone = u.phone,
                    Role = u.role
                })
                .ToList();

            UsersGrid.ItemsSource = users;
        }
    }
}