using System.Linq;
using System.Windows.Controls;
using UP_Murtazin.DB;

namespace UP_Murtazin.Pages
{
    public partial class ModemsPage : Page
    {
        private UP_MurtazinEntities dbContext;

        public ModemsPage()
        {
            InitializeComponent();
            dbContext = new UP_MurtazinEntities();
            LoadData();
        }

        private void LoadData()
        {
            var modems = dbContext.kit_online_id
                .Select(k => new
                {
                    KitOnlineId = k.kit_online_id1,
                    PaymentType = k.payment_type,
                    Status = k.status,
                    Company = k.company
                })
                .ToList();

            ModemsGrid.ItemsSource = modems;
        }
    }
}