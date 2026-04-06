using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP_Murtazin.DB;

namespace UP_Murtazin.Okna
{
    public partial class EditMachineWindow : Window
    {
        private UP_MurtazinEntities dbContext;
        private vending_machines currentMachine;
        private bool isEditMode = false;

        // Хранилища для ComboBox элементов
        private List<ComboBoxItem> clientItems;
        private List<ComboBoxItem> managerItems;
        private List<ComboBoxItem> engineerItems;
        private List<ComboBoxItem> technicianItems;

        public EditMachineWindow(UP_MurtazinEntities context)
        {
            InitializeComponent();
            dbContext = context;
            InitializeComboBoxStores();
            LoadComboBoxData();
        }

        public EditMachineWindow(UP_MurtazinEntities context, vending_machines machine) : this(context)
        {
            currentMachine = machine;
            isEditMode = true;
            Title = "Редактирование торгового автомата";
            LoadMachineData();
        }

        private void InitializeComboBoxStores()
        {
            clientItems = new List<ComboBoxItem>();
            managerItems = new List<ComboBoxItem>();
            engineerItems = new List<ComboBoxItem>();
            technicianItems = new List<ComboBoxItem>();
        }

        private void LoadComboBoxData()
        {
            try
            {
                // === Производители (компании) ===
                var companies = dbContext.companies?.Select(c => c.company)
                    .Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList() ?? new List<string>();
                CmbManufacturer.Items.Clear();
                CmbManufacturer.Items.Add("Не выбрано");
                foreach (var c in companies) CmbManufacturer.Items.Add(c);
                if (CmbManufacturer.Items.Count > 0) CmbManufacturer.SelectedIndex = 0;

                // === Модели из rfid_loading ===
                var models = dbContext.rfid_loading?.Select(rl => rl.model)
                    .Where(m => !string.IsNullOrEmpty(m)).Distinct().ToList() ?? new List<string>();
                CmbModel.Items.Clear();
                CmbModel.Items.Add("Не указана");
                foreach (var m in models) CmbModel.Items.Add(m);
                if (CmbModel.Items.Count > 0) CmbModel.SelectedIndex = 0;

                // === Пользователи ===
                var allUsers = dbContext.users?.ToList() ?? new List<users>();

                // Клиенты (все пользователи)
                clientItems.Clear();
                clientItems.Add(new ComboBoxItem { Content = "Не задан", Tag = null });
                foreach (var u in allUsers)
                    clientItems.Add(new ComboBoxItem { Content = u.full_name, Tag = u.user_id });

                // Очищаем и заполняем ComboBox
                CmbClient.Items.Clear();
                foreach (var item in clientItems)
                    CmbClient.Items.Add(item);
                CmbClient.SelectedIndex = 0;

                // Менеджеры (is_manager = true)
                managerItems.Clear();
                managerItems.Add(new ComboBoxItem { Content = "Не задан", Tag = null });
                foreach (var u in allUsers.Where(x => x.is_manager == "true" || x.is_manager == "1"))
                    managerItems.Add(new ComboBoxItem { Content = u.full_name, Tag = u.user_id });

                CmbManager.Items.Clear();
                foreach (var item in managerItems)
                    CmbManager.Items.Add(item);
                CmbManager.SelectedIndex = 0;

                // Инженеры (is_engineer = true)
                engineerItems.Clear();
                engineerItems.Add(new ComboBoxItem { Content = "Не задан", Tag = null });
                foreach (var u in allUsers.Where(x => x.is_engineer == "true" || x.is_engineer == "1"))
                    engineerItems.Add(new ComboBoxItem { Content = u.full_name, Tag = u.user_id });

                CmbEngineer.Items.Clear();
                foreach (var item in engineerItems)
                    CmbEngineer.Items.Add(item);
                CmbEngineer.SelectedIndex = 0;

                // Техники (role == "Персонал" или is_operator == true)
                technicianItems.Clear();
                technicianItems.Add(new ComboBoxItem { Content = "Не задан", Tag = null });
                foreach (var u in allUsers.Where(x => x.role == "Персонал" || x.is_operator == "true"))
                    technicianItems.Add(new ComboBoxItem { Content = u.full_name, Tag = u.user_id });

                CmbTechnician.Items.Clear();
                foreach (var item in technicianItems)
                    CmbTechnician.Items.Add(item);
                CmbTechnician.SelectedIndex = 0;

                // === Режим работы (из work_modes) ===
                var workModes = dbContext.work_modes?.Select(w => w.work_mode)
                    .Where(w => !string.IsNullOrEmpty(w)).ToList() ?? new List<string>();
                CmbOperationMode.Items.Clear();
                CmbOperationMode.Items.Add("Не выбрано");
                foreach (var wm in workModes) CmbOperationMode.Items.Add(wm);
                if (CmbOperationMode.Items.Count > 0) CmbOperationMode.SelectedIndex = 0;

                // === Приоритет обслуживания (из service_priorities) ===
                var priorities = dbContext.service_priorities?.Select(p => p.service_priority)
                    .Where(p => !string.IsNullOrEmpty(p)).ToList() ?? new List<string>();
                CmbServicePriority.Items.Clear();
                CmbServicePriority.Items.Add("Не выбрано");
                foreach (var pr in priorities) CmbServicePriority.Items.Add(pr);
                if (CmbServicePriority.Items.Count > 0) CmbServicePriority.SelectedIndex = 0;

                // === Шаблон критических значений ===
                var criticalTemplates = dbContext.critical_threshold_templates?.Select(t => t.critical_threshold_template)
                    .Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>();
                CmbCriticalValuesTemplate.Items.Clear();
                CmbCriticalValuesTemplate.Items.Add("Не установлен");
                foreach (var ct in criticalTemplates) CmbCriticalValuesTemplate.Items.Add(ct);
                if (CmbCriticalValuesTemplate.Items.Count > 0) CmbCriticalValuesTemplate.SelectedIndex = 0;

                // === Шаблон уведомлений ===
                var notificationTemplates = dbContext.notification_templates?.Select(t => t.notification_template)
                    .Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>();
                CmbNotificationTemplate.Items.Clear();
                CmbNotificationTemplate.Items.Add("Не установлен");
                foreach (var nt in notificationTemplates) CmbNotificationTemplate.Items.Add(nt);
                if (CmbNotificationTemplate.Items.Count > 0) CmbNotificationTemplate.SelectedIndex = 0;

                // === Тип платежа ===
                var paymentTypes = dbContext.payment_types?.Select(pt => pt.payment_type)
                    .Where(pt => !string.IsNullOrEmpty(pt)).ToList() ?? new List<string>();
                CmbPaymentType.Items.Clear();
                CmbPaymentType.Items.Add("Не выбрано");
                foreach (var pt in paymentTypes) CmbPaymentType.Items.Add(pt);
                if (CmbPaymentType.Items.Count > 0) CmbPaymentType.SelectedIndex = 0;

                // === Часовые пояса ===
                CmbTimezone.Items.Clear();
                CmbTimezone.Items.Add("Не выбран");
                foreach (var tz in new[] { "UTC+3", "UTC+2", "UTC+4", "UTC+5", "UTC+6" })
                    CmbTimezone.Items.Add(tz);
                if (CmbTimezone.Items.Count > 0) CmbTimezone.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMachineData()
        {
            if (currentMachine == null) return;

            try
            {
                // Основные поля
                TxtName.Text = currentMachine.name ?? "";
                TxtPlace.Text = currentMachine.place ?? "";
                TxtMachineNumber.Text = currentMachine.serial_number?.ToString() ?? "";
                TxtID.Text = currentMachine.vending_machine_id ?? "";

                TxtRfidService.Text = currentMachine.rfid_service ?? "";
                TxtRfidCollection.Text = currentMachine.rfid_cash_collection ?? "";
                TxtRfidLoading.Text = currentMachine.rfid_loading ?? "";

                // Получаем kit_online_id
                var kit = dbContext.kit_online_id
                    .FirstOrDefault(k => k.vending_machine_id == currentMachine.vending_machine_id);

                if (kit != null)
                {
                    TxtKitOnlineId.Text = kit.kit_online_id1 ?? "";
                    TxtWorkingHours.Text = kit.working_hours ?? "";

                    // Выбор производителя
                    if (!string.IsNullOrEmpty(kit.company))
                    {
                        for (int i = 0; i < CmbManufacturer.Items.Count; i++)
                        {
                            if (CmbManufacturer.Items[i].ToString() == kit.company)
                            {
                                CmbManufacturer.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    // Выбор приоритета обслуживания
                    if (!string.IsNullOrEmpty(kit.service_priority))
                    {
                        for (int i = 0; i < CmbServicePriority.Items.Count; i++)
                        {
                            if (CmbServicePriority.Items[i].ToString() == kit.service_priority)
                            {
                                CmbServicePriority.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    // Выбор шаблона критических значений
                    if (!string.IsNullOrEmpty(kit.critical_threshold_template))
                    {
                        for (int i = 0; i < CmbCriticalValuesTemplate.Items.Count; i++)
                        {
                            if (CmbCriticalValuesTemplate.Items[i].ToString() == kit.critical_threshold_template)
                            {
                                CmbCriticalValuesTemplate.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    // Выбор шаблона уведомлений
                    if (!string.IsNullOrEmpty(kit.notification_template))
                    {
                        for (int i = 0; i < CmbNotificationTemplate.Items.Count; i++)
                        {
                            if (CmbNotificationTemplate.Items[i].ToString() == kit.notification_template)
                            {
                                CmbNotificationTemplate.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    // Выбор типа платежа
                    if (!string.IsNullOrEmpty(kit.payment_type))
                    {
                        for (int i = 0; i < CmbPaymentType.Items.Count; i++)
                        {
                            if (CmbPaymentType.Items[i].ToString() == kit.payment_type)
                            {
                                CmbPaymentType.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    // Выбор менеджера
                    if (!string.IsNullOrEmpty(kit.manager))
                    {
                        for (int i = 0; i < CmbManager.Items.Count; i++)
                        {
                            var item = CmbManager.Items[i] as ComboBoxItem;
                            if (item != null && item.Tag?.ToString() == kit.manager)
                            {
                                CmbManager.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    // Выбор инженера
                    if (!string.IsNullOrEmpty(kit.engineer))
                    {
                        for (int i = 0; i < CmbEngineer.Items.Count; i++)
                        {
                            var item = CmbEngineer.Items[i] as ComboBoxItem;
                            if (item != null && item.Tag?.ToString() == kit.engineer)
                            {
                                CmbEngineer.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }

                // Клиент (user_id из vending_machines)
                if (!string.IsNullOrEmpty(currentMachine.user_id))
                {
                    for (int i = 0; i < CmbClient.Items.Count; i++)
                    {
                        var item = CmbClient.Items[i] as ComboBoxItem;
                        if (item != null && item.Tag?.ToString() == currentMachine.user_id)
                        {
                            CmbClient.SelectedIndex = i;
                            break;
                        }
                    }
                }

                // Техник
                if (!string.IsNullOrEmpty(currentMachine.technician))
                {
                    for (int i = 0; i < CmbTechnician.Items.Count; i++)
                    {
                        var item = CmbTechnician.Items[i] as ComboBoxItem;
                        if (item != null && item.Tag?.ToString() == currentMachine.technician)
                        {
                            CmbTechnician.SelectedIndex = i;
                            break;
                        }
                    }
                }

                // Модель из rfid_loading
                if (!string.IsNullOrEmpty(currentMachine.rfid_loading))
                {
                    var rfidLoad = dbContext.rfid_loading
                        .FirstOrDefault(rl => rl.rfid_loading1 == currentMachine.rfid_loading);

                    if (rfidLoad != null && !string.IsNullOrEmpty(rfidLoad.model))
                    {
                        for (int i = 0; i < CmbModel.Items.Count; i++)
                        {
                            if (CmbModel.Items[i].ToString() == rfidLoad.model)
                            {
                                CmbModel.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                }

                // Адрес из rfid_cash_collections
                if (!string.IsNullOrEmpty(currentMachine.rfid_cash_collection))
                {
                    var rfidCash = dbContext.rfid_cash_collections
                        .FirstOrDefault(rc => rc.rfid_cash_collection == currentMachine.rfid_cash_collection);

                    if (rfidCash != null)
                    {
                        if (!string.IsNullOrEmpty(rfidCash.location))
                            TxtAddress.Text = rfidCash.location;
                        if (!string.IsNullOrEmpty(rfidCash.notes))
                            TxtCoordinates.Text = rfidCash.notes;
                        if (!string.IsNullOrEmpty(rfidCash.work_mode))
                        {
                            for (int i = 0; i < CmbOperationMode.Items.Count; i++)
                            {
                                if (CmbOperationMode.Items[i].ToString() == rfidCash.work_mode)
                                {
                                    CmbOperationMode.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Координаты и часовой пояс из rfid_services
                if (!string.IsNullOrEmpty(currentMachine.rfid_service))
                {
                    var rfidSvc = dbContext.rfid_services
                        .FirstOrDefault(rs => rs.rfid_service == currentMachine.rfid_service);

                    if (rfidSvc != null)
                    {
                        if (!string.IsNullOrEmpty(rfidSvc.coordinates))
                            TxtCoordinates.Text = rfidSvc.coordinates;
                        if (!string.IsNullOrEmpty(rfidSvc.timezone))
                        {
                            for (int i = 0; i < CmbTimezone.Items.Count; i++)
                            {
                                if (CmbTimezone.Items[i].ToString() == rfidSvc.timezone)
                                {
                                    CmbTimezone.SelectedIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных автомата: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveMachineData()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtName.Text))
                {
                    MessageBox.Show("Название обязательно", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool isNew = currentMachine == null;
                if (isNew)
                {
                    currentMachine = new vending_machines();
                    currentMachine.vending_machine_id = !string.IsNullOrWhiteSpace(TxtID.Text)
                        ? TxtID.Text : Guid.NewGuid().ToString();
                    dbContext.vending_machines.Add(currentMachine);
                }

                // === Основные поля ===
                currentMachine.name = TxtName.Text;
                currentMachine.place = TxtPlace.Text;
                currentMachine.serial_number = string.IsNullOrWhiteSpace(TxtMachineNumber.Text)
                    ? (double?)null : double.Parse(TxtMachineNumber.Text);
                currentMachine.rfid_service = TxtRfidService.Text;
                currentMachine.rfid_cash_collection = TxtRfidCollection.Text;
                currentMachine.rfid_loading = TxtRfidLoading.Text;


                // Клиент и техник
                currentMachine.user_id = GetSelectedUserId(CmbClient);
                currentMachine.technician = GetSelectedUserId(CmbTechnician);

                // === kit_online_id ===
                var kit = dbContext.kit_online_id
                    .FirstOrDefault(k => k.vending_machine_id == currentMachine.vending_machine_id);

                if (kit == null && !string.IsNullOrWhiteSpace(TxtKitOnlineId.Text))
                {
                    kit = new kit_online_id();
                    kit.vending_machine_id = currentMachine.vending_machine_id;
                    dbContext.kit_online_id.Add(kit);
                }

                if (kit != null)
                {
                    kit.kit_online_id1 = TxtKitOnlineId.Text;
                    kit.working_hours = TxtWorkingHours.Text;
                    kit.company = GetSelectedText(CmbManufacturer);
                    kit.service_priority = GetSelectedText(CmbServicePriority);
                    kit.critical_threshold_template = GetSelectedText(CmbCriticalValuesTemplate);
                    kit.notification_template = GetSelectedText(CmbNotificationTemplate);
                    kit.payment_type = GetSelectedText(CmbPaymentType);
                    kit.manager = GetSelectedUserId(CmbManager);
                    kit.engineer = GetSelectedUserId(CmbEngineer);
                }

                // === Обновление rfid_loading ===
                if (!string.IsNullOrWhiteSpace(TxtRfidLoading.Text))
                {
                    var rfidLoad = dbContext.rfid_loading
                        .FirstOrDefault(rl => rl.rfid_loading1 == TxtRfidLoading.Text);

                    if (rfidLoad == null)
                    {
                        rfidLoad = new rfid_loading();
                        rfidLoad.vending_machine_id = currentMachine.vending_machine_id;
                        rfidLoad.rfid_loading1 = TxtRfidLoading.Text;
                        dbContext.rfid_loading.Add(rfidLoad);
                    }
                    rfidLoad.model = GetSelectedText(CmbModel);
                }

                // === Обновление rfid_cash_collections ===
                if (!string.IsNullOrWhiteSpace(TxtRfidCollection.Text))
                {
                    var rfidCash = dbContext.rfid_cash_collections
                        .FirstOrDefault(rc => rc.rfid_cash_collection == TxtRfidCollection.Text);

                    if (rfidCash == null)
                    {
                        rfidCash = new rfid_cash_collections();
                        rfidCash.vending_machine_id = currentMachine.vending_machine_id;
                        rfidCash.rfid_cash_collection = TxtRfidCollection.Text;
                        dbContext.rfid_cash_collections.Add(rfidCash);
                    }
                    rfidCash.location = TxtAddress.Text;
                    rfidCash.notes = TxtCoordinates.Text;
                    rfidCash.work_mode = GetSelectedText(CmbOperationMode);
                }

                // === Обновление rfid_services ===
                if (!string.IsNullOrWhiteSpace(TxtRfidService.Text))
                {
                    var rfidSvc = dbContext.rfid_services
                        .FirstOrDefault(rs => rs.rfid_service == TxtRfidService.Text);

                    if (rfidSvc == null)
                    {
                        rfidSvc = new rfid_services();
                        rfidSvc.vending_machine_id = currentMachine.vending_machine_id;
                        rfidSvc.rfid_service = TxtRfidService.Text;
                        dbContext.rfid_services.Add(rfidSvc);
                    }
                    rfidSvc.coordinates = TxtCoordinates.Text;
                    rfidSvc.timezone = GetSelectedText(CmbTimezone);
                }

                Helpers.NotificationManager.Instance.ShowNotification(
                    "Успешно",
                    isNew ? "Автомат успешно создан" : "Данные успешно обновлены",
                    Models.NotificationType.Info);


                dbContext.SaveChanges();
                MessageBox.Show(isNew ? "Автомат успешно создан" : "Данные успешно обновлены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка сохранения: {ex.Message}\n{ex.InnerException?.Message}", "Ошибка",
                //    MessageBoxButton.OK, MessageBoxImage.Error);

                Helpers.NotificationManager.Instance.ShowNotification(
            "Ошибка",
            $"Не удалось сохранить данные: {ex.Message}",
            Models.NotificationType.Critical);
            }
        }

        // Вспомогательные методы
        private string GetSelectedText(ComboBox cmb)
        {
            if (cmb.SelectedIndex <= 0)
                return null;

            return cmb.SelectedItem?.ToString();
        }

        private string GetSelectedUserId(ComboBox cmb)
        {
            if (cmb.SelectedItem is ComboBoxItem cbi && cbi.Tag != null)
                return cbi.Tag.ToString();
            return null;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveMachineData();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}