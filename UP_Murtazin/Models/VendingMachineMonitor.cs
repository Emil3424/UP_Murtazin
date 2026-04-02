using System;
using System.ComponentModel;
using System.Windows.Media;

namespace UP_Murtazin.Models
{
    public class VendingMachineMonitor : INotifyPropertyChanged
    {
        private int _number;
        private string _machineId;
        private string _name;
        private string _model;
        private string _location;
        private string _address;
        private string _operatorName;
        private string _operatorIcon;
        private string _signalIcon;
        private string _signalStrength;
        private string _status;
        private Brush _statusColor;
        private string _connectionType;
        private int _signalStrengthValue;
        private DateTime _lastConnection;
        private decimal _totalIncome;
        private int _loadPercentage;
        private Brush _loadColor;
        private string _loadText;
        private int _productLoad;
        private int _maxProducts;
        private decimal _cashAmount;
        private int _coinCount;
        private DateTime? _lastEventTime;
        private string _lastEventType;
        private bool _isCoinAcceptorWorking;
        private bool _isBillAcceptorWorking;
        private bool _isCardReaderWorking;
        private bool _isDispenserWorking;
        private string _coinStatus;
        private string _billStatus;
        private string _cardStatus;
        private string _dispenserStatus;
        private int _additionalInfo1;
        private int _additionalInfo2;

        // Новые свойства для монитора
        public int Number { get => _number; set { _number = value; OnPropertyChanged(nameof(Number)); } }
        public string MachineId { get => _machineId; set { _machineId = value; OnPropertyChanged(nameof(MachineId)); } }
        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }
        public string Model { get => _model; set { _model = value; OnPropertyChanged(nameof(Model)); } }
        public string Location { get => _location; set { _location = value; OnPropertyChanged(nameof(Location)); } }
        public string Address { get => _address; set { _address = value; OnPropertyChanged(nameof(Address)); } }
        public string OperatorName { get => _operatorName; set { _operatorName = value; OnPropertyChanged(nameof(OperatorName)); } }
        public string OperatorIcon { get => _operatorIcon; set { _operatorIcon = value; OnPropertyChanged(nameof(OperatorIcon)); } }
        public string SignalIcon { get => _signalIcon; set { _signalIcon = value; OnPropertyChanged(nameof(SignalIcon)); } }
        public string SignalStrength { get => _signalStrength; set { _signalStrength = value; OnPropertyChanged(nameof(SignalStrength)); } }
        public string Status { get => _status; set { _status = value; OnPropertyChanged(nameof(Status)); } }
        public Brush StatusColor { get => _statusColor; set { _statusColor = value; OnPropertyChanged(nameof(StatusColor)); } }
        public string ConnectionType { get => _connectionType; set { _connectionType = value; OnPropertyChanged(nameof(ConnectionType)); } }
        public int SignalStrengthValue { get => _signalStrengthValue; set { _signalStrengthValue = value; OnPropertyChanged(nameof(SignalStrengthValue)); } }
        public DateTime LastConnection { get => _lastConnection; set { _lastConnection = value; OnPropertyChanged(nameof(LastConnection)); } }
        
        // Денежные средства из rfid_services.total_income
        public decimal TotalIncome { get => _totalIncome; set { _totalIncome = value; OnPropertyChanged(nameof(TotalIncome)); } }
        
        // Загрузка
        public int LoadPercentage { get => _loadPercentage; set { _loadPercentage = value; OnPropertyChanged(nameof(LoadPercentage)); } }
        public Brush LoadColor { get => _loadColor; set { _loadColor = value; OnPropertyChanged(nameof(LoadColor)); } }
        public string LoadText { get => _loadText; set { _loadText = value; OnPropertyChanged(nameof(LoadText)); } }
        
        public int ProductLoad { get => _productLoad; set { _productLoad = value; OnPropertyChanged(nameof(ProductLoad)); } }
        public int MaxProducts { get => _maxProducts; set { _maxProducts = value; OnPropertyChanged(nameof(MaxProducts)); } }
        public decimal CashAmount { get => _cashAmount; set { _cashAmount = value; OnPropertyChanged(nameof(CashAmount)); } }
        public int CoinCount { get => _coinCount; set { _coinCount = value; OnPropertyChanged(nameof(CoinCount)); } }
        
        // События
        public DateTime? LastEventTime { get => _lastEventTime; set { _lastEventTime = value; OnPropertyChanged(nameof(LastEventTime)); } }
        public string LastEventType { get => _lastEventType; set { _lastEventType = value; OnPropertyChanged(nameof(LastEventType)); } }
        
        // Оборудование
        public bool IsCoinAcceptorWorking { get => _isCoinAcceptorWorking; set { _isCoinAcceptorWorking = value; OnPropertyChanged(nameof(IsCoinAcceptorWorking)); } }
        public bool IsBillAcceptorWorking { get => _isBillAcceptorWorking; set { _isBillAcceptorWorking = value; OnPropertyChanged(nameof(IsBillAcceptorWorking)); } }
        public bool IsCardReaderWorking { get => _isCardReaderWorking; set { _isCardReaderWorking = value; OnPropertyChanged(nameof(IsCardReaderWorking)); } }
        public bool IsDispenserWorking { get => _isDispenserWorking; set { _isDispenserWorking = value; OnPropertyChanged(nameof(IsDispenserWorking)); } }
        
        // Статусы оборудования в виде текста
        public string CoinStatus { get => _coinStatus; set { _coinStatus = value; OnPropertyChanged(nameof(CoinStatus)); } }
        public string BillStatus { get => _billStatus; set { _billStatus = value; OnPropertyChanged(nameof(BillStatus)); } }
        public string CardStatus { get => _cardStatus; set { _cardStatus = value; OnPropertyChanged(nameof(CardStatus)); } }
        public string DispenserStatus { get => _dispenserStatus; set { _dispenserStatus = value; OnPropertyChanged(nameof(DispenserStatus)); } }
        
        public int AdditionalInfo1 { get => _additionalInfo1; set { _additionalInfo1 = value; OnPropertyChanged(nameof(AdditionalInfo1)); } }
        public int AdditionalInfo2 { get => _additionalInfo2; set { _additionalInfo2 = value; OnPropertyChanged(nameof(AdditionalInfo2)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}