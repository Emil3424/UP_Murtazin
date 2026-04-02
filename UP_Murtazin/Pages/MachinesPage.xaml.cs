using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using UP_Murtazin.DB;
using UP_Murtazin.Okna;

namespace UP_Murtazin.Pages
{
    public partial class MachinesPage : Page
    {
        private UP_MurtazinEntities dbContext;
        private ObservableCollection<MachineDisplayModel> allMachines;
        private ObservableCollection<MachineDisplayModel> filteredMachines;
        private int currentPage = 1;
        private int pageSize = 50;
        private int totalPages = 1;
        private bool isTileView = false;

        // Переменные для перемещения плиток
        private Border currentDraggedTile;
        private Point dragStartPoint;
        private bool isDragging = false;
        private Dictionary<int, Point> tilePositions = new Dictionary<int, Point>();
        private int tileWidth = 280;
        private int tileHeight = 200;
        private int tileMargin = 15;
        private int tilesPerRow = 3;

        public MachinesPage()
        {
            InitializeComponent();
            dbContext = new UP_MurtazinEntities();

            allMachines = new ObservableCollection<MachineDisplayModel>();
            filteredMachines = new ObservableCollection<MachineDisplayModel>();

            this.SizeChanged += MachinesPage_SizeChanged;
        }

        private void MachinesPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (isTileView && TilesCanvas != null && TilesCanvas.ActualWidth > 0)
            {
                ArrangeTiles();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var query = from vm in dbContext.vending_machines
                            join ko in dbContext.kit_online_id on vm.kit_online_id equals ko.kit_online_id1 into koJoin
                            from ko in koJoin.DefaultIfEmpty()
                            join rl in dbContext.rfid_loading on vm.rfid_loading equals rl.rfid_loading1 into rlJoin
                            from rl in rlJoin.DefaultIfEmpty()
                            join rc in dbContext.rfid_cash_collections on vm.rfid_cash_collection equals rc.rfid_cash_collection into rcJoin
                            from rc in rcJoin.DefaultIfEmpty()
                            join comp in dbContext.companies on ko.company equals comp.company into compJoin
                            from comp in compJoin.DefaultIfEmpty()
                            select new
                            {
                                vm,
                                ko,
                                rl,
                                rc,
                                comp
                            };

                var machinesList = query.ToList();

                var machines = machinesList.Select(x => new MachineDisplayModel
                {
                    Id = x.vm.serial_number.HasValue ? (int)x.vm.serial_number.Value : 0,
                    Name = x.vm.name ?? "Не указано",
                    Model = x.rl != null && x.rl.model != null ? x.rl.model : "Не указана",
                    CompanyName = x.comp != null && x.comp.company != null ? x.comp.company : "Не указана",
                    SerialNumber = x.vm.serial_number.HasValue ? x.vm.serial_number.Value.ToString() : "Нет данных",
                    Location = x.rc != null && x.rc.location != null ? x.rc.location : (x.vm.place ?? "Не указано"),
                    InstallDate = x.vm.install_date ?? "Не указана",
                    IsBlocked = false
                }).ToList();

                allMachines = new ObservableCollection<MachineDisplayModel>(machines);
                filteredMachines = new ObservableCollection<MachineDisplayModel>(allMachines);
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ArrangeTiles()
        {
            if (TilesItemsControl?.Items == null) return;
            if (TilesCanvas == null) return;

            try
            {
                double canvasWidth = TilesCanvas.ActualWidth > 0 ? TilesCanvas.ActualWidth : 800;

                // Вычисляем количество плиток в строке
                tilesPerRow = Math.Max(1, (int)((canvasWidth - 20) / (tileWidth + tileMargin)));

                int row = 0;
                int col = 0;

                for (int i = 0; i < TilesItemsControl.Items.Count; i++)
                {
                    var item = TilesItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                    if (item != null)
                    {
                        var border = VisualTreeHelper.GetChild(item, 0) as Border;
                        if (border != null)
                        {
                            // Вычисляем стандартную позицию по сетке
                            double defaultX = col * (tileWidth + tileMargin) + tileMargin;
                            double defaultY = row * (tileHeight + tileMargin) + tileMargin;

                            var transform = border.RenderTransform as TranslateTransform;
                            if (transform == null)
                            {
                                transform = new TranslateTransform();
                                border.RenderTransform = transform;
                            }

                            // Проверяем, есть ли сохраненная позиция
                            int machineId = (int)border.Tag;
                            if (tilePositions.ContainsKey(machineId))
                            {
                                // Используем сохраненную позицию
                                var savedPos = tilePositions[machineId];
                                transform.X = savedPos.X;
                                transform.Y = savedPos.Y;
                            }
                            else
                            {
                                // Используем стандартную позицию
                                transform.X = defaultX;
                                transform.Y = defaultY;
                            }

                            col++;
                            if (col >= tilesPerRow)
                            {
                                col = 0;
                                row++;
                            }
                        }
                    }
                }

                // Устанавливаем высоту Canvas
                int totalRows = (int)Math.Ceiling((double)TilesItemsControl.Items.Count / tilesPerRow);
                TilesCanvas.Height = totalRows * (tileHeight + tileMargin) + tileMargin;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка ArrangeTiles: {ex.Message}");
            }
        }

        private void UpdateDisplay()
        {
            if (filteredMachines == null) return;

            TotalCountText.Text = $"Всего найдено {filteredMachines.Count} шт.";

            totalPages = filteredMachines.Count > 0 ? (int)Math.Ceiling((double)filteredMachines.Count / pageSize) : 1;
            if (totalPages == 0) totalPages = 1;

            if (currentPage > totalPages) currentPage = totalPages;
            if (currentPage < 1) currentPage = 1;

            var pageData = filteredMachines
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Обновляем таблицу
            MachinesGrid.ItemsSource = pageData;

            // Обновляем плитки
            TilesItemsControl.ItemsSource = pageData;

            // После обновления ItemsSource, располагаем плитки
            if (isTileView)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ArrangeTiles();
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }

            int startRecord = filteredMachines.Count > 0 ? (currentPage - 1) * pageSize + 1 : 0;
            int endRecord = filteredMachines.Count > 0 ? Math.Min(currentPage * pageSize, filteredMachines.Count) : 0;

            PaginationText.Text = filteredMachines.Count == 0
                ? "Запись с 0 по 0 из 0 записей"
                : $"Запись с {startRecord} по {endRecord} из {filteredMachines.Count} записей";

            CurrentPageText.Text = currentPage.ToString();
            TotalPagesText.Text = totalPages.ToString();

            PrevButton.IsEnabled = currentPage > 1;
            NextButton.IsEnabled = currentPage < totalPages;
        }

        private void TableViewToggle_Click(object sender, RoutedEventArgs e)
        {
            isTileView = false;
            TableViewToggle.IsChecked = true;
            TileViewToggle.IsChecked = false;
            TableViewBorder.Visibility = Visibility.Visible;
            TileViewBorder.Visibility = Visibility.Collapsed;
        }

        private void TileViewToggle_Click(object sender, RoutedEventArgs e)
        {
            isTileView = true;
            TableViewToggle.IsChecked = false;
            TileViewToggle.IsChecked = true;
            TableViewBorder.Visibility = Visibility.Collapsed;
            TileViewBorder.Visibility = Visibility.Visible;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ArrangeTiles();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // Обработчики для перемещения плиток
        private void Tile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                currentDraggedTile = border;
                dragStartPoint = e.GetPosition(TilesCanvas);
                isDragging = false;

                Panel.SetZIndex(border, 100);
                border.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Tile_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentDraggedTile != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(TilesCanvas);
                Vector delta = currentPoint - dragStartPoint;

                if (Math.Abs(delta.X) > 5 || Math.Abs(delta.Y) > 5)
                {
                    isDragging = true;

                    var transform = currentDraggedTile.RenderTransform as TranslateTransform;
                    if (transform != null)
                    {
                        transform.X += delta.X;
                        transform.Y += delta.Y;
                        dragStartPoint = currentPoint;
                    }
                }
                e.Handled = true;
            }
        }

        private void Tile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (currentDraggedTile != null)
            {
                currentDraggedTile.ReleaseMouseCapture();

                if (isDragging)
                {
                    var transform = currentDraggedTile.RenderTransform as TranslateTransform;
                    if (transform != null && currentDraggedTile.Tag != null)
                    {
                        int machineId = (int)currentDraggedTile.Tag;
                        tilePositions[machineId] = new Point(transform.X, transform.Y);
                    }
                }

                Panel.SetZIndex(currentDraggedTile, 0);
                currentDraggedTile = null;
                isDragging = false;
                e.Handled = true;
            }
        }

        // Двойной клик для редактирования
        private void Tile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag != null && !isDragging)
            {
                int id = (int)border.Tag;
                var machine = dbContext.vending_machines.FirstOrDefault(vm => vm.serial_number == id);
                if (machine != null)
                {
                    var editWindow = new EditMachineWindow(dbContext, machine);
                    editWindow.ShowDialog();
                    LoadData();
                }
                e.Handled = true;
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentDraggedTile != null)
            {
                currentDraggedTile.ReleaseMouseCapture();
                Panel.SetZIndex(currentDraggedTile, 0);
                currentDraggedTile = null;
                isDragging = false;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e) { }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { }

        // Остальные методы (PageSizeCombo_SelectionChanged, FilterButton_Click, и т.д.)
        // ... (оставляем без изменений)

        private void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageSizeCombo?.SelectedItem is ComboBoxItem item && int.TryParse(item.Content.ToString(), out int newSize))
            {
                pageSize = newSize;
                currentPage = 1;
                UpdateDisplay();
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = true;
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filtered = allMachines.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(FilterName?.Text))
                {
                    string filterName = FilterName.Text.ToLower();
                    filtered = filtered.Where(m => m.Name != null && m.Name.ToLower().Contains(filterName));
                }

                if (!string.IsNullOrWhiteSpace(FilterModel?.Text))
                {
                    string filterModel = FilterModel.Text.ToLower();
                    filtered = filtered.Where(m => m.Model != null && m.Model.ToLower().Contains(filterModel));
                }

                if (!string.IsNullOrWhiteSpace(FilterCompany?.Text))
                {
                    string filterCompany = FilterCompany.Text.ToLower();
                    filtered = filtered.Where(m => m.CompanyName != null && m.CompanyName.ToLower().Contains(filterCompany));
                }

                if (!string.IsNullOrWhiteSpace(FilterAddress?.Text))
                {
                    string filterAddress = FilterAddress.Text.ToLower();
                    filtered = filtered.Where(m => m.Location != null && m.Location.ToLower().Contains(filterAddress));
                }

                filteredMachines = new ObservableCollection<MachineDisplayModel>(filtered);
                currentPage = 1;
                UpdateDisplay();
                FilterPopup.IsOpen = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            if (FilterName != null) FilterName.Text = string.Empty;
            if (FilterModel != null) FilterModel.Text = string.Empty;
            if (FilterCompany != null) FilterCompany.Text = string.Empty;
            if (FilterAddress != null) FilterAddress.Text = string.Empty;

            filteredMachines = new ObservableCollection<MachineDisplayModel>(allMachines);
            currentPage = 1;
            UpdateDisplay();
            FilterPopup.IsOpen = false;
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdateDisplay();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                UpdateDisplay();
            }
        }

        private void BlockButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int id = (int)button.Tag;
                var machine = allMachines.FirstOrDefault(m => m.Id == id);
                if (machine != null && !machine.IsBlocked)
                {
                    machine.IsBlocked = true;

                    // Показываем уведомление
                    Helpers.NotificationManager.Instance.ShowNotification(
                        "Блокировка",
                        $"Автомат {machine.Name} (ID: {id}) заблокирован",
                        Models.NotificationType.Warning);

                    UpdateDisplay();
                }
            }
        }

        private void UnblockButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int id = (int)button.Tag;
                var machine = allMachines.FirstOrDefault(m => m.Id == id);
                if (machine != null && machine.IsBlocked)
                {
                    machine.IsBlocked = false;

                    // Показываем уведомление
                    Helpers.NotificationManager.Instance.ShowNotification(
                        "Разблокировка",
                        $"Автомат {machine.Name} (ID: {id}) разблокирован",
                        Models.NotificationType.Info);

                    UpdateDisplay();
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag != null)
            {
                int id = (int)button.Tag;
                var machine = dbContext.vending_machines.FirstOrDefault(vm => vm.serial_number == id);
                if (machine != null)
                {
                    var editWindow = new EditMachineWindow(dbContext, machine);
                    editWindow.ShowDialog();
                    LoadData();
                }
            }
        }

        private void AddMachine_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new EditMachineWindow(dbContext);
            addWindow.ShowDialog();
            LoadData();
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FilterIndex = 1,
                    DefaultExt = "csv",
                    FileName = $"Торговые_автоматы_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    DataTable dt = new DataTable("Торговые автоматы");
                    dt.Columns.Add("ID", typeof(int));
                    dt.Columns.Add("Название автомата", typeof(string));
                    dt.Columns.Add("Модель", typeof(string));
                    dt.Columns.Add("Компания", typeof(string));
                    dt.Columns.Add("Модем", typeof(string));
                    dt.Columns.Add("Адрес / Место", typeof(string));
                    dt.Columns.Add("Дата установки", typeof(string));

                    foreach (var machine in filteredMachines)
                    {
                        dt.Rows.Add(
                            machine.Id,
                            machine.Name,
                            machine.Model,
                            machine.CompanyName,
                            machine.SerialNumber,
                            machine.Location,
                            machine.InstallDate
                        );
                    }

                    ExportToCsv(dt, saveDialog.FileName);
                    MessageBox.Show($"Данные экспортированы в файл: {saveDialog.FileName}",
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToCsv(DataTable dt, string filePath)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sw.Write($"\"{dt.Columns[i].ColumnName}\"");
                    if (i < dt.Columns.Count - 1) sw.Write(";");
                }
                sw.WriteLine();

                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        sw.Write($"\"{row[i]}\"");
                        if (i < dt.Columns.Count - 1) sw.Write(";");
                    }
                    sw.WriteLine();
                }
            }
        }

        private void EditTile_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.Tag != null)
            {
                int id = (int)menuItem.Tag;
                var machine = dbContext.vending_machines.FirstOrDefault(vm => vm.serial_number == id);
                if (machine != null)
                {
                    var editWindow = new EditMachineWindow(dbContext, machine);
                    editWindow.ShowDialog();
                    LoadData();
                }
            }
        }

        private void BlockTile_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.Tag != null)
            {
                int id = (int)menuItem.Tag;
                var machine = allMachines.FirstOrDefault(m => m.Id == id);
                if (machine != null && !machine.IsBlocked)
                {
                    machine.IsBlocked = true;
                    UpdateDisplay();
                }
            }
        }

        private void UnblockTile_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.Tag != null)
            {
                int id = (int)menuItem.Tag;
                var machine = allMachines.FirstOrDefault(m => m.Id == id);
                if (machine != null && machine.IsBlocked)
                {
                    machine.IsBlocked = false;
                    UpdateDisplay();
                }
            }
        }

        private void DeleteTile_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.Tag != null)
            {
                int id = (int)menuItem.Tag;
                if (MessageBox.Show($"Удалить автомат ID: {id}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var machine = dbContext.vending_machines.FirstOrDefault(vm => vm.serial_number == id);
                    if (machine != null)
                    {
                        dbContext.vending_machines.Remove(machine);
                        dbContext.SaveChanges();
                        LoadData();
                    }
                }
            }
        }
        // Простой экспорт в CSV
        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    FilterIndex = 1,
                    DefaultExt = "csv",
                    FileName = $"Торговые_автоматы_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var dataTable = PrepareDataForExport();
                    ExportToCsvSimple(dataTable, saveDialog.FileName);

                    MessageBox.Show($"Данные экспортированы в CSV!\n\nФайл: {saveDialog.FileName}",
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Экспорт в HTML
        private void ExportHtml_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "HTML files (*.html)|*.html",
                    FilterIndex = 1,
                    DefaultExt = "html",
                    FileName = $"Торговые_автоматы_{DateTime.Now:yyyyMMdd_HHmmss}.html"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var dataTable = PrepareDataForExport();
                    ExportToHtmlSimple(dataTable, saveDialog.FileName);

                    MessageBox.Show($"Данные экспортированы в HTML!\n\nФайл: {saveDialog.FileName}",
                        "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Экспорт в PDF (простой способ через печать)
        private void ExportPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataTable = PrepareDataForExport();

                // Создаем HTML для печати
                string html = GenerateHtmlForPrint(dataTable);

                string tempFile = System.IO.Path.GetTempPath() + $"export_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                System.IO.File.WriteAllText(tempFile, html, System.Text.Encoding.UTF8);

                // Открываем в браузере для печати
                System.Diagnostics.Process.Start(tempFile);

                MessageBox.Show("HTML файл открыт в браузере.\n\n" +
                                "Для сохранения как PDF:\n" +
                                "1. Нажмите Ctrl+P\n" +
                                "2. Выберите 'Сохранить как PDF'\n" +
                                "3. Укажите место сохранения",
                                "Экспорт в PDF", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Кнопка для открытия меню экспорта
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.ContextMenu != null)
            {
                button.ContextMenu.IsOpen = true;
            }
        }

        // Подготовка данных для экспорта
        private DataTable PrepareDataForExport()
        {
            DataTable dt = new DataTable("Торговые автоматы");
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Название автомата", typeof(string));
            dt.Columns.Add("Модель", typeof(string));
            dt.Columns.Add("Компания", typeof(string));
            dt.Columns.Add("Модем", typeof(string));
            dt.Columns.Add("Адрес / Место", typeof(string));
            dt.Columns.Add("Дата установки", typeof(string));

            foreach (var machine in filteredMachines)
            {
                dt.Rows.Add(
                    machine.Id,
                    machine.Name,
                    machine.Model,
                    machine.CompanyName,
                    machine.SerialNumber,
                    machine.Location,
                    machine.InstallDate
                );
            }

            return dt;
        }

        // Простой экспорт в CSV
        private void ExportToCsvSimple(DataTable dt, string filePath)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                // Записываем заголовки
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sw.Write($"\"{dt.Columns[i].ColumnName}\"");
                    if (i < dt.Columns.Count - 1) sw.Write(";");
                }
                sw.WriteLine();

                // Записываем данные
                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        string value = row[i]?.ToString() ?? "";
                        value = value.Replace("\"", "\"\"");
                        sw.Write($"\"{value}\"");
                        if (i < dt.Columns.Count - 1) sw.Write(";");
                    }
                    sw.WriteLine();
                }
            }
        }

        // Простой экспорт в HTML
        private void ExportToHtmlSimple(DataTable dt, string filePath)
        {
            string html = $@"<!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <title>Торговые автоматы</title>
        <style>
            body {{
                font-family: 'Segoe UI', Arial, sans-serif;
                margin: 40px;
                background-color: #f5f7fa;
            }}
            .container {{
                max-width: 1200px;
                margin: 0 auto;
                background: white;
                border-radius: 8px;
                box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                overflow: hidden;
            }}
            h1 {{
                background: #2c3e50;
                color: white;
                margin: 0;
                padding: 20px;
                font-size: 24px;
            }}
            .info {{
                padding: 15px 20px;
                background: #ecf0f1;
                color: #7f8c8d;
                border-bottom: 1px solid #ddd;
                font-size: 14px;
            }}
            table {{
                width: 100%;
                border-collapse: collapse;
            }}
            th {{
                background: #3498db;
                color: white;
                padding: 12px;
                text-align: left;
                font-weight: 600;
            }}
            td {{
                padding: 10px 12px;
                border-bottom: 1px solid #e0e0e0;
            }}
            tr:hover {{
                background-color: #f8f9fa;
            }}
            .footer {{
                padding: 15px 20px;
                background: #ecf0f1;
                text-align: center;
                color: #7f8c8d;
                font-size: 12px;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <h1>Торговые автоматы</h1>
            <div class='info'>
                📊 Всего записей: {dt.Rows.Count} | 📅 Дата выгрузки: {DateTime.Now:dd.MM.yyyy HH:mm:ss}
            </div>
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Название автомата</th>
                        <th>Модель</th>
                        <th>Компания</th>
                        <th>Модем</th>
                        <th>Адрес / Место</th>
                        <th>Дата установки</th>
                    </tr>
                </thead>
                <tbody>";

            foreach (DataRow row in dt.Rows)
            {
                html += "<tr>";
                foreach (DataColumn col in dt.Columns)
                {
                    string value = row[col]?.ToString() ?? "";
                    value = System.Security.SecurityElement.Escape(value);
                    html += $"<td>{value}</td>";
                }
                html += "</tr>";
            }

            html += $@"
                </tbody>
            </table>
            <div class='footer'>
                Сгенерировано автоматически | Торговые автоматы
            </div>
        </div>
    </body>
    </html>";

            System.IO.File.WriteAllText(filePath, html, System.Text.Encoding.UTF8);
        }

        // Генерация HTML для печати (PDF)
        // Генерация HTML для печати (PDF)
        private string GenerateHtmlForPrint(DataTable dt)
        {
            System.Text.StringBuilder html = new System.Text.StringBuilder();

            html.AppendLine(@"<!DOCTYPE html>");
            html.AppendLine(@"<html>");
            html.AppendLine(@"<head>");
            html.AppendLine(@"<meta charset='UTF-8'>");
            html.AppendLine(@"<title>Торговые автоматы</title>");
            html.AppendLine(@"<style>");
            html.AppendLine(@"@media print {");
            html.AppendLine(@"    body { margin: 0; padding: 20px; }");
            html.AppendLine(@"    table { page-break-inside: avoid; }");
            html.AppendLine(@"    .page-break { page-break-before: always; }");
            html.AppendLine(@"}");
            html.AppendLine(@"body {");
            html.AppendLine(@"    font-family: 'Segoe UI', Arial, sans-serif;");
            html.AppendLine(@"    margin: 20px;");
            html.AppendLine(@"    font-size: 12px;");
            html.AppendLine(@"}");
            html.AppendLine(@"h1 {");
            html.AppendLine(@"    color: #2c3e50;");
            html.AppendLine(@"    border-bottom: 2px solid #3498db;");
            html.AppendLine(@"    padding-bottom: 10px;");
            html.AppendLine(@"    font-size: 18px;");
            html.AppendLine(@"}");
            html.AppendLine(@".info {");
            html.AppendLine(@"    margin: 15px 0;");
            html.AppendLine(@"    color: #7f8c8d;");
            html.AppendLine(@"    font-size: 11px;");
            html.AppendLine(@"}");
            html.AppendLine(@"table {");
            html.AppendLine(@"    width: 100%;");
            html.AppendLine(@"    border-collapse: collapse;");
            html.AppendLine(@"    margin-top: 15px;");
            html.AppendLine(@"}");
            html.AppendLine(@"th {");
            html.AppendLine(@"    background: #3498db;");
            html.AppendLine(@"    color: white;");
            html.AppendLine(@"    padding: 6px;");
            html.AppendLine(@"    text-align: left;");
            html.AppendLine(@"    font-size: 11px;");
            html.AppendLine(@"}");
            html.AppendLine(@"td {");
            html.AppendLine(@"    padding: 6px;");
            html.AppendLine(@"    border-bottom: 1px solid #ddd;");
            html.AppendLine(@"    font-size: 10px;");
            html.AppendLine(@"}");
            html.AppendLine(@".footer {");
            html.AppendLine(@"    margin-top: 20px;");
            html.AppendLine(@"    text-align: center;");
            html.AppendLine(@"    color: #95a5a6;");
            html.AppendLine(@"    font-size: 9px;");
            html.AppendLine(@"}");
            html.AppendLine(@"</style>");
            html.AppendLine(@"</head>");
            html.AppendLine(@"<body>");
            html.AppendLine($"<h1>Торговые автоматы</h1>");
            html.AppendLine($"<div class='info'>");
            html.AppendLine($"    <strong>Всего записей:</strong> {dt.Rows.Count} |");
            html.AppendLine($"    <strong>Дата выгрузки:</strong> {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            html.AppendLine($"</div>");
            html.AppendLine(@"<table>");
            html.AppendLine(@"    <thead>");
            html.AppendLine(@"        <tr>");
            html.AppendLine(@"            <th>ID</th>");
            html.AppendLine(@"            <th>Название автомата</th>");
            html.AppendLine(@"            <th>Модель</th>");
            html.AppendLine(@"            <th>Компания</th>");
            html.AppendLine(@"            <th>Модем</th>");
            html.AppendLine(@"            <th>Адрес / Место</th>");
            html.AppendLine(@"            <th>Дата установки</th>");
            html.AppendLine(@"        </tr>");
            html.AppendLine(@"    </thead>");
            html.AppendLine(@"    <tbody>");

            foreach (DataRow row in dt.Rows)
            {
                html.AppendLine(@"        <tr>");
                foreach (DataColumn col in dt.Columns)
                {
                    string value = row[col]?.ToString() ?? "";
                    value = System.Security.SecurityElement.Escape(value);
                    html.AppendLine($"            <td>{value}</td>");
                }
                html.AppendLine(@"        </tr>");
            }

            html.AppendLine(@"    </tbody>");
            html.AppendLine(@"</table>");
            html.AppendLine(@"<div class='footer'>");
            html.AppendLine(@"    Сгенерировано автоматически | Торговые автоматы");
            html.AppendLine(@"</div>");
            html.AppendLine(@"</body>");
            html.AppendLine(@"</html>");

            return html.ToString();
        }
        // Сброс позиций плиток в исходное состояние
        private void ResetTilePositions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Очищаем сохраненные позиции
                tilePositions.Clear();

                // Перестраиваем расположение плиток
                if (isTileView)
                {
                    ArrangeTiles();
                }

                MessageBox.Show("Позиции плиток восстановлены в исходное состояние",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сброса позиций: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class MachineDisplayModel : INotifyPropertyChanged
    {
        private bool _isBlocked;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string CompanyName { get; set; }
        public string SerialNumber { get; set; }
        public string Location { get; set; }
        public string InstallDate { get; set; }

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