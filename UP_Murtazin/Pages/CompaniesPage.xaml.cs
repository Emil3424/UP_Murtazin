using System.Linq;
using System.Windows.Controls;
using UP_Murtazin.DB;

namespace UP_Murtazin.Pages
{
    public partial class CompaniesPage : Page
    {
        private UP_MurtazinEntities dbContext;

        public CompaniesPage()
        {
            InitializeComponent();
            dbContext = new UP_MurtazinEntities();
            LoadData();
        }

        private void LoadData()
        {
            var companies = dbContext.companies
                .Select(c => new { CompanyName = c.company })
                .ToList();

            CompaniesGrid.ItemsSource = companies;
        }
    }
}