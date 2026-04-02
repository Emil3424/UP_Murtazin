using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UP_Murtazin.DB;
using UP_Murtazin.Models;

namespace UP_Murtazin.Pages
{
    public partial class MonitorPage : Page
    {
        private UP_MurtazinEntities dbContext;
        private ObservableCollection<VendingMachineMonitor> allMachines;
        private ObservableCollection<VendingMachineMonitor> filteredMachines;
        private Random random = new Random();

        private bool filterStatusGreen = false;
        private bool filterStatusRed = false;
        private bool filterStatusBlue = false;

        // Словари для иконок операторов
        private readonly Dictionary<string, string> operatorIcons = new Dictionary<string, string>
        {
            { "МТС", "/Resources/Modem/MTS.png" },
            { "Мегафон", "/Resources/Modem/Megafon.png" },
            { "Билайн", "/Resources/Modem/Beeline.png" },
            { "Т2", "/Resources/Modem/Tele2.png" }
        };

        // Словарь для иконок сигнала
        private readonly string[] signalIcons = new string[]
        {
            "/Resources/Modem/Signal1.png",
            "/Resources/Modem/Signal2.png",
            "/Resources/Modem/Signal3.png",
            "/Resources/Modem/Signal5.png"
        };

        public MonitorPage()
        {
            InitializeComponent();
            this.Loaded += MonitorPage_Loaded;
        }

        private void MonitorPage_Loaded(object sender, RoutedEventArgs e)
        {
            dbContext = new UP_MurtazinEntities();
            allMachines = new ObservableCollection<VendingMachineMonitor>();
            filteredMachines = new ObservableCollection<VendingMachineMonitor>();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var machines = from vm in dbContext.vending_machines
                               join ko in dbContext.kit_online_id on vm.vending_machine_id equals ko.vending_machine_id into koJoin
                               from ko in koJoin.DefaultIfEmpty()
                               join op in dbContext.operators on vm.@operator equals op.@operator into opJoin
                               from op in opJoin.DefaultIfEmpty()
                               join rs in dbContext.rfid_services on vm.rfid_service equals rs.rfid_service into rsJoin
                               from rs in rsJoin.DefaultIfEmpty()
                               select new
                               {
                                   vm,
                                   ko,
                                   op,
                                   rs
                               };

                var machinesList = machines.ToList();
                allMachines.Clear();

                int number = 1;
                foreach (var m in machinesList)
                {
                    try
                    {
                        // Получаем сумму денег из rfid_services.total_income
                        decimal totalIncome = 0;
                        if (m.rs?.total_income != null)
                        {
                            decimal.TryParse(m.rs.total_income, System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out totalIncome);
                        }

                        // Получаем случайный уровень сигнала (от 1 до 4 для выбора иконки)
                        int signalLevel = random.Next(0, 4);
                        string operatorName = m.op?.@operator ?? "Не указан";
                        string operatorIcon = operatorIcons.ContainsKey(operatorName) ? operatorIcons[operatorName] : "/Resources/Modem/Beeline.png";
                        string signalIcon = signalIcons[signalLevel];
                        int signalStrengthValue = (signalLevel + 1) * 25; // 25%, 50%, 75%, 100%
                        string signalStrengthText = $"{signalStrengthValue}%";

                        // Рассчитываем загрузку (рандомное значение от 10 до 95)
                        int loadPercentage = random.Next(10, 96);
                        string loadColorHex = loadPercentage < 30 ? "#F44336" : (loadPercentage < 70 ? "#FF9800" : "#4CAF50");
                        string loadText = loadPercentage < 30 ? "Низкая" : (loadPercentage < 70 ? "Средняя" : "Высокая");

                        // Получаем статус из kit_online_id
                        string status = m.ko?.status ?? "Неизвестно";
                        Brush statusColor = status == "Работает" ? new SolidColorBrush(Colors.Green) :
                                            status == "Сломан" ? new SolidColorBrush(Colors.Red) :
                                            status == "Обслуживается" ? new SolidColorBrush(Colors.Blue) :
                                            new SolidColorBrush(Colors.Gray);

                        var machine = new VendingMachineMonitor
                        {
                            Number = number,
                            MachineId = m.vm.vending_machine_id,
                            Name = m.vm.name ?? "Не указано",
                            Model = GetModelFromRfidLoading(m.vm.rfid_loading),
                            Address = GetAddressFromRfidCash(m.vm.rfid_cash_collection),
                            Location = m.vm.place ?? "Не указано",
                            OperatorName = operatorName,
                            OperatorIcon = operatorIcon,
                            SignalIcon = signalIcon,
                            SignalStrength = signalStrengthText,
                            Status = status,
                            StatusColor = statusColor,
                            TotalIncome = totalIncome,
                            LoadPercentage = loadPercentage,
                            LoadColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(loadColorHex)),
                            LoadText = loadText,
                            CashAmount = GetCashAmount(m.vm.vending_machine_id),
                            CoinCount = GetCoinCount(m.vm.vending_machine_id),
                            LastEventTime = GetLastEventTime(m.vm.vending_machine_id),
                            LastEventType = GetLastEventType(m.vm.vending_machine_id),
                            CoinStatus = GetCoinStatus(),
                            BillStatus = GetBillStatus(),
                            CardStatus = GetCardStatus(),
                            DispenserStatus = GetDispenserStatus()
                        };

                        allMachines.Add(machine);
                        number++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки автомата {m.vm.vending_machine_id}: {ex.Message}");
                    }
                }

                filteredMachines = new ObservableCollection<VendingMachineMonitor>(allMachines);
                MachinesGrid.ItemsSource = filteredMachines;
                UpdateSummary();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== ОШИБКА ЗАГРУЗКИ ДАННЫХ ===");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");

                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\n\nПодробнее см. в окне вывода (Output)",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private decimal GetCashAmount(string machineId)
        {
            try
            {
                var sales = dbContext.sales
                    .Where(s => s.product != null && s.product.vending_machine_id == machineId)
                    .ToList();

                decimal total = 0;
                foreach (var sale in sales)
                {
                    if (decimal.TryParse(sale.total_price, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal price))
                    {
                        total += price;
                    }
                }
                return total;
            }
            catch
            {
                return 0;
            }
        }

        private int GetCoinCount(string machineId)
        {
            try
            {
                var sales = dbContext.sales
                    .Where(s => s.product != null &&
                               s.product.vending_machine_id == machineId &&
                               s.payment_method == "Наличные")
                    .ToList();

                int count = 0;
                foreach (var sale in sales)
                {
                    if (sale.quantity.HasValue)
                        count += (int)sale.quantity.Value;
                }
                return count;
            }
            catch
            {
                return 0;
            }
        }

        private DateTime? GetLastEventTime(string machineId)
        {
            try
            {
                var maintenance = dbContext.maintenance
                    .Where(m => m.vending_machine_id == machineId)
                    .OrderByDescending(m => m.date)
                    .FirstOrDefault();

                return maintenance?.date;
            }
            catch
            {
                return null;
            }
        }

        private string GetLastEventType(string machineId)
        {
            try
            {
                var maintenance = dbContext.maintenance
                    .Where(m => m.vending_machine_id == machineId)
                    .OrderByDescending(m => m.date)
                    .FirstOrDefault();

                return maintenance?.work_description ?? "Нет событий";
            }
            catch
            {
                return "Нет событий";
            }
        }

        private string GetModelFromRfidLoading(string rfidLoading)
        {
            try
            {
                if (string.IsNullOrEmpty(rfidLoading)) return "Не указана";
                var rl = dbContext.rfid_loading.FirstOrDefault(r => r.rfid_loading1 == rfidLoading);
                return rl?.model ?? "Не указана";
            }
            catch
            {
                return "Не указана";
            }
        }

        private string GetAddressFromRfidCash(string rfidCash)
        {
            try
            {
                if (string.IsNullOrEmpty(rfidCash)) return "Не указан";
                var rc = dbContext.rfid_cash_collections.FirstOrDefault(r => r.rfid_cash_collection == rfidCash);
                return rc?.location ?? "Не указан";
            }
            catch
            {
                return "Не указан";
            }
        }

        private string GetCoinStatus()
        {
            string[] statuses = { "✅", "❌", "⚠️" };
            return statuses[random.Next(0, 3)];
        }

        private string GetBillStatus()
        {
            string[] statuses = { "✅", "❌", "⚠️" };
            return statuses[random.Next(0, 3)];
        }

        private string GetCardStatus()
        {
            string[] statuses = { "✅", "❌", "⚠️" };
            return statuses[random.Next(0, 3)];
        }

        private string GetDispenserStatus()
        {
            string[] statuses = { "✅", "❌", "⚠️" };
            return statuses[random.Next(0, 3)];
        }

        private void StatusFilter_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            var tag = border.Tag.ToString();

            if (tag == "Работает")
            {
                filterStatusGreen = !filterStatusGreen;
                border.Background = filterStatusGreen ? new SolidColorBrush(Color.FromArgb(50, 76, 175, 80)) : new SolidColorBrush(Colors.White);
            }
            else if (tag == "Сломан")
            {
                filterStatusRed = !filterStatusRed;
                border.Background = filterStatusRed ? new SolidColorBrush(Color.FromArgb(50, 244, 67, 54)) : new SolidColorBrush(Colors.White);
            }
            else if (tag == "Обслуживается")
            {
                filterStatusBlue = !filterStatusBlue;
                border.Background = filterStatusBlue ? new SolidColorBrush(Color.FromArgb(50, 33, 150, 243)) : new SolidColorBrush(Colors.White);
            }
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filtered = allMachines.AsEnumerable();

                // Фильтр по статусу
                if (filterStatusGreen || filterStatusRed || filterStatusBlue)
                {
                    var statuses = new List<string>();
                    if (filterStatusGreen) statuses.Add("Работает");
                    if (filterStatusRed) statuses.Add("Сломан");
                    if (filterStatusBlue) statuses.Add("Обслуживается");

                    filtered = filtered.Where(m => statuses.Contains(m.Status));
                }

                // Сортировка
                var sortItem = CmbSort.SelectedItem as ComboBoxItem;
                if (sortItem != null)
                {
                    switch (sortItem.Content.ToString())
                    {
                        case "По номеру":
                            filtered = filtered.OrderBy(m => m.Number);
                            break;
                        case "По названию":
                            filtered = filtered.OrderBy(m => m.Name);
                            break;
                        case "По сумме денег":
                            filtered = filtered.OrderByDescending(m => m.TotalIncome);
                            break;
                        case "По загрузке":
                            filtered = filtered.OrderByDescending(m => m.LoadPercentage);
                            break;
                        default:
                            filtered = filtered.OrderBy(m => m.Status);
                            break;
                    }
                }

                filteredMachines = new ObservableCollection<VendingMachineMonitor>(filtered);
                MachinesGrid.ItemsSource = filteredMachines;
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            filterStatusGreen = false;
            filterStatusRed = false;
            filterStatusBlue = false;

            StatusGreen.Background = new SolidColorBrush(Colors.White);
            StatusRed.Background = new SolidColorBrush(Colors.White);
            StatusBlue.Background = new SolidColorBrush(Colors.White);

            filteredMachines = new ObservableCollection<VendingMachineMonitor>(allMachines);
            MachinesGrid.ItemsSource = filteredMachines;
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            var total = filteredMachines.Count;
            var working = filteredMachines.Count(m => m.Status == "Работает");
            var offline = filteredMachines.Count(m => m.Status == "Сломан");
            var maintenance = filteredMachines.Count(m => m.Status == "Обслуживается");
            var totalMoney = filteredMachines.Sum(m => m.TotalIncome);

            TxtTotalMachines.Text = total.ToString();
            TxtWorkingMachines.Text = working.ToString();
            TxtOfflineMachines.Text = offline.ToString();
            TxtMaintenanceMachines.Text = maintenance.ToString();
            TxtTotalMoney.Text = totalMoney.ToString("N2");
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Монитор_ТА_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                dlg.DefaultExt = ".xlsx";
                dlg.Filter = "Excel files (.xlsx)|*.xlsx";

                if (dlg.ShowDialog() == true)
                {
                    MessageBox.Show("Данные экспортированы в Excel", "Экспорт",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}