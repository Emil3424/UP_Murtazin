using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UP_Murtazin.Models
{
    [Table("vending_machines")]
    public class MachineModel : INotifyPropertyChanged
    {
        [Key]
        [Column("vending_machine_id")]
        public string VendingMachineId { get; set; }

        [Column("serial_number")]
        public double? SerialNumber { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("rfid_cash_collection")]
        public string RfidCashCollection { get; set; }

        [Column("rfid_loading")]
        public string RfidLoading { get; set; }

        [Column("kit_online_id")]
        public string KitOnlineId { get; set; }

        [Column("rfid_service")]
        public string RfidService { get; set; }

        [Column("install_date")]
        public string InstallDate { get; set; }

        [Column("place")]
        public string Place { get; set; }

        [Column("operator")]
        public string Operator { get; set; }

        [Column("technician")]
        public string Technician { get; set; }

        [Column("last_maintenance_date")]
        public string LastMaintenanceDate { get; set; }

        // Свойства для отображения в таблице
        [NotMapped]
        public int Id
        {
            get => (int)(SerialNumber ?? 0);
            set => SerialNumber = value;
        }

        [NotMapped]
        public string CompanyName { get; set; }

        [NotMapped]
        public string Model { get; set; }

        [NotMapped]
        public string Location { get; set; }

        [NotMapped]
        public string SerialNumberStr { get; set; }

        [NotMapped]
        private bool _isBlocked;

        [NotMapped]
        public bool IsBlocked
        {
            get => _isBlocked;
            set
            {
                _isBlocked = value;
                OnPropertyChanged(nameof(IsBlocked));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}