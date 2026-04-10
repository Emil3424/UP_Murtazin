using Microsoft.Data.SqlClient;
using UP_Murtazin.Web.Models;

namespace UP_Murtazin.Web.Services;

public sealed class SqlDataService
{
    private readonly string _connectionString;

    public SqlDataService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string DefaultConnection is missing.");
    }

    public async Task<UserSession?> AuthenticateAsync(string email, string password)
    {
        const string query = """
            SELECT TOP 1 user_id, full_name, email, role, phone, image, passsword
            FROM users
            WHERE email = @email
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@email", email);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var hash = reader["passsword"] as string;
        if (!PasswordHasher.VerifyPassword(password, hash))
        {
            return null;
        }

        return new UserSession
        {
            UserId = reader["user_id"].ToString() ?? string.Empty,
            FullName = reader["full_name"].ToString() ?? string.Empty,
            Email = reader["email"].ToString() ?? string.Empty,
            Role = reader["role"].ToString() ?? string.Empty,
            Phone = reader["phone"].ToString() ?? string.Empty,
            ImageBase64 = reader["image"] as string
        };
    }

    public async Task<ProfileViewModel?> GetProfileAsync(string userId)
    {
        const string query = """
            SELECT TOP 1 full_name, email, role, phone, image
            FROM users
            WHERE user_id = @userId
            """;
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@userId", userId);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new ProfileViewModel
        {
            FullName = reader["full_name"].ToString() ?? string.Empty,
            Email = reader["email"].ToString() ?? string.Empty,
            Role = reader["role"].ToString() ?? string.Empty,
            Phone = reader["phone"].ToString() ?? "Not specified",
            ImageBase64 = reader["image"] as string
        };
    }

    public async Task<HomeDashboardViewModel> GetHomeDashboardAsync()
    {
        var model = new HomeDashboardViewModel { LastUpdate = DateTime.Now };

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // machine statuses from kit_online_id
        const string statusQuery = "SELECT status FROM kit_online_id";
        await using (var statusCommand = new SqlCommand(statusQuery, connection))
        await using (var statusReader = await statusCommand.ExecuteReaderAsync())
        {
            while (await statusReader.ReadAsync())
            {
                string status = statusReader["status"].ToString() ?? string.Empty;
                model.TotalMachines++;
                if (status == "Работает") model.WorkingMachines++;
                else if (status == "Обслуживается") model.MaintenanceMachines++;
                else if (status == "Сломан") model.BrokenMachines++;
            }
        }

        // money in machines from rfid_services.total_income
        const string incomeQuery = "SELECT total_income FROM rfid_services WHERE total_income IS NOT NULL";
        await using (var incomeCommand = new SqlCommand(incomeQuery, connection))
        await using (var incomeReader = await incomeCommand.ExecuteReaderAsync())
        {
            while (await incomeReader.ReadAsync())
            {
                if (decimal.TryParse(incomeReader["total_income"].ToString(), out decimal value))
                {
                    model.MoneyInMachines += value;
                }
            }
        }
        model.ChangeInMachines = model.MoneyInMachines * 0.3m;

        // sales aggregates
        var salesByDate = new Dictionary<DateTime, SalesPointViewModel>();
        const string salesQuery = "SELECT timestamp, total_price, quantity FROM sales WHERE timestamp IS NOT NULL";
        await using (var salesCommand = new SqlCommand(salesQuery, connection))
        await using (var salesReader = await salesCommand.ExecuteReaderAsync())
        {
            while (await salesReader.ReadAsync())
            {
                if (!DateTime.TryParse(salesReader["timestamp"].ToString(), out DateTime date))
                {
                    continue;
                }
                date = date.Date;
                decimal price = 0;
                decimal.TryParse(salesReader["total_price"].ToString(), out price);
                decimal quantity = 0;
                decimal.TryParse(salesReader["quantity"].ToString(), out quantity);
                if (!salesByDate.TryGetValue(date, out SalesPointViewModel? point))
                {
                    point = new SalesPointViewModel { Date = date };
                    salesByDate.Add(date, point);
                }
                point.TotalSum += price;
                point.TotalQuantity += quantity;
            }
        }

        var sortedSales = salesByDate.Values.OrderByDescending(x => x.Date).ToList();
        if (sortedSales.Count > 0)
        {
            model.RevenueToday = sortedSales[0].TotalSum;
            model.CollectedToday = model.RevenueToday * 0.8m;
        }
        if (sortedSales.Count > 1)
        {
            model.RevenueYesterday = sortedSales[1].TotalSum;
            model.CollectedYesterday = model.RevenueYesterday * 0.8m;
        }
        model.Last10DaysSales = salesByDate.Values.OrderBy(x => x.Date).TakeLast(10).ToList();

        // maintenance today/yesterday
        const string maintenanceQuery = "SELECT date FROM maintenance WHERE date IS NOT NULL";
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        await using (var maintenanceCommand = new SqlCommand(maintenanceQuery, connection))
        await using (var maintenanceReader = await maintenanceCommand.ExecuteReaderAsync())
        {
            while (await maintenanceReader.ReadAsync())
            {
                if (!DateTime.TryParse(maintenanceReader["date"].ToString(), out DateTime date))
                {
                    continue;
                }
                if (date.Date == today) model.ServicedToday++;
                if (date.Date == yesterday) model.ServicedYesterday++;
            }
        }

        model.News =
        [
            new NewsItemViewModel { Date = "29.01.25", Title = "Терминалы KiPops получили эквайринг от Сберга" },
            new NewsItemViewModel { Date = "31.12.24", Title = "Новогоднее поздравление от KIT Vending / KIT Shop" },
            new NewsItemViewModel { Date = "28.12.24", Title = "Ставки НДС 5% и 7% для УСН" },
            new NewsItemViewModel { Date = "04.12.24", Title = "Релиз новой CRM-системы KIT Shop" }
        ];

        return model;
    }

    public async Task<List<MonitorMachineViewModel>> GetMonitorMachinesAsync()
    {
        const string query = """
            SELECT vm.vending_machine_id, vm.name, vm.place, vm.rfid_loading, vm.rfid_cash_collection,
                   ko.status, op.[operator] AS operator_name, rs.total_income
            FROM vending_machines vm
            LEFT JOIN kit_online_id ko ON vm.vending_machine_id = ko.vending_machine_id
            LEFT JOIN operators op ON vm.[operator] = op.[operator]
            LEFT JOIN rfid_services rs ON vm.rfid_service = rs.rfid_service
            ORDER BY vm.serial_number
            """;

        var result = new List<MonitorMachineViewModel>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        int number = 1;
        while (await reader.ReadAsync())
        {
            string rfidLoading = reader["rfid_loading"].ToString() ?? string.Empty;
            string rfidCash = reader["rfid_cash_collection"].ToString() ?? string.Empty;
            var model = await GetModelAndAddressAsync(connection, rfidLoading, rfidCash);
            decimal income = 0;
            decimal.TryParse(reader["total_income"].ToString(), out income);
            int load = (number * 17) % 100;
            if (load < 10) load += 10;

            result.Add(new MonitorMachineViewModel
            {
                Number = number++,
                MachineId = reader["vending_machine_id"].ToString() ?? string.Empty,
                Name = reader["name"].ToString() ?? "Not specified",
                Model = model.Model,
                Address = model.Address,
                Location = reader["place"].ToString() ?? "Not specified",
                OperatorName = reader["operator_name"].ToString() ?? "Not specified",
                Status = reader["status"].ToString() ?? "Unknown",
                TotalIncome = income,
                LoadPercentage = load
            });
        }
        return result;
    }

    private async Task<(string Model, string Address)> GetModelAndAddressAsync(SqlConnection connection, string rfidLoading, string rfidCash)
    {
        string model = "Not specified";
        string address = "Not specified";
        if (!string.IsNullOrWhiteSpace(rfidLoading))
        {
            await using var cmd = new SqlCommand("SELECT TOP 1 model FROM rfid_loading WHERE rfid_loading = @id", connection);
            cmd.Parameters.AddWithValue("@id", rfidLoading);
            var value = await cmd.ExecuteScalarAsync();
            model = value?.ToString() ?? model;
        }
        if (!string.IsNullOrWhiteSpace(rfidCash))
        {
            await using var cmd = new SqlCommand("SELECT TOP 1 location FROM rfid_cash_collections WHERE rfid_cash_collection = @id", connection);
            cmd.Parameters.AddWithValue("@id", rfidCash);
            var value = await cmd.ExecuteScalarAsync();
            address = value?.ToString() ?? address;
        }
        return (model, address);
    }

    public async Task<List<MachineViewModel>> GetMachinesAsync()
    {
        const string query = """
            SELECT vending_machine_id, serial_number, name, place, install_date, user_id,
                   rfid_cash_collection, rfid_loading, rfid_service, technician
            FROM vending_machines
            ORDER BY serial_number
            """;
        var result = new List<MachineViewModel>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new MachineViewModel
            {
                VendingMachineId = reader["vending_machine_id"].ToString(),
                SerialNumber = reader["serial_number"] as double?,
                Name = reader["name"].ToString() ?? string.Empty,
                Place = reader["place"].ToString(),
                InstallDate = reader["install_date"].ToString(),
                UserId = reader["user_id"].ToString(),
                RfidCashCollection = reader["rfid_cash_collection"].ToString(),
                RfidLoading = reader["rfid_loading"].ToString(),
                RfidService = reader["rfid_service"].ToString(),
                Technician = reader["technician"].ToString()
            });
        }

        return result;
    }

    public async Task<MachineViewModel?> GetMachineAsync(string id)
    {
        const string query = """
            SELECT TOP 1 vending_machine_id, serial_number, name, place, install_date, user_id,
                   rfid_cash_collection, rfid_loading, rfid_service, technician
            FROM vending_machines WHERE vending_machine_id = @id
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new MachineViewModel
        {
            VendingMachineId = reader["vending_machine_id"].ToString(),
            SerialNumber = reader["serial_number"] as double?,
            Name = reader["name"].ToString() ?? string.Empty,
            Place = reader["place"].ToString(),
            InstallDate = reader["install_date"].ToString(),
            UserId = reader["user_id"].ToString(),
            RfidCashCollection = reader["rfid_cash_collection"].ToString(),
            RfidLoading = reader["rfid_loading"].ToString(),
            RfidService = reader["rfid_service"].ToString(),
            Technician = reader["technician"].ToString()
        };
    }

    public async Task SaveMachineAsync(MachineViewModel model)
    {
        bool isNew = string.IsNullOrWhiteSpace(model.VendingMachineId);
        string id = model.VendingMachineId ?? Guid.NewGuid().ToString();

        const string insertSql = """
            INSERT INTO vending_machines
            (vending_machine_id, serial_number, name, place, install_date, user_id, rfid_cash_collection, rfid_loading, rfid_service, technician)
            VALUES
            (@id, @serial, @name, @place, @installDate, @userId, @rfidCashCollection, @rfidLoading, @rfidService, @technician)
            """;
        const string updateSql = """
            UPDATE vending_machines
            SET serial_number = @serial, name = @name, place = @place, install_date = @installDate, user_id = @userId,
                rfid_cash_collection = @rfidCashCollection, rfid_loading = @rfidLoading, rfid_service = @rfidService, technician = @technician
            WHERE vending_machine_id = @id
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(isNew ? insertSql : updateSql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@serial", (object?)model.SerialNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("@name", model.Name);
        command.Parameters.AddWithValue("@place", (object?)model.Place ?? DBNull.Value);
        command.Parameters.AddWithValue("@installDate", (object?)model.InstallDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@userId", (object?)model.UserId ?? DBNull.Value);
        command.Parameters.AddWithValue("@rfidCashCollection", (object?)model.RfidCashCollection ?? DBNull.Value);
        command.Parameters.AddWithValue("@rfidLoading", (object?)model.RfidLoading ?? DBNull.Value);
        command.Parameters.AddWithValue("@rfidService", (object?)model.RfidService ?? DBNull.Value);
        command.Parameters.AddWithValue("@technician", (object?)model.Technician ?? DBNull.Value);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteMachineAsync(string id)
    {
        const string query = "DELETE FROM vending_machines WHERE vending_machine_id = @id";
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<UserViewModel>> GetUsersAsync()
    {
        const string query = "SELECT user_id, email, full_name, phone, role, is_manager, is_engineer, is_operator FROM users ORDER BY full_name";
        var result = new List<UserViewModel>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new UserViewModel
            {
                UserId = reader["user_id"].ToString(),
                Email = reader["email"].ToString() ?? string.Empty,
                FullName = reader["full_name"].ToString() ?? string.Empty,
                Phone = reader["phone"].ToString(),
                Role = reader["role"].ToString(),
                IsManager = (reader["is_manager"].ToString() ?? "").Equals("true", StringComparison.OrdinalIgnoreCase),
                IsEngineer = (reader["is_engineer"].ToString() ?? "").Equals("true", StringComparison.OrdinalIgnoreCase),
                IsOperator = (reader["is_operator"].ToString() ?? "").Equals("true", StringComparison.OrdinalIgnoreCase)
            });
        }
        return result;
    }

    public async Task<UserViewModel?> GetUserAsync(string id)
    {
        const string query = "SELECT TOP 1 user_id, email, full_name, phone, role, is_manager, is_engineer, is_operator FROM users WHERE user_id = @id";
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new UserViewModel
        {
            UserId = reader["user_id"].ToString(),
            Email = reader["email"].ToString() ?? string.Empty,
            FullName = reader["full_name"].ToString() ?? string.Empty,
            Phone = reader["phone"].ToString(),
            Role = reader["role"].ToString(),
            IsManager = (reader["is_manager"].ToString() ?? "").Equals("true", StringComparison.OrdinalIgnoreCase),
            IsEngineer = (reader["is_engineer"].ToString() ?? "").Equals("true", StringComparison.OrdinalIgnoreCase),
            IsOperator = (reader["is_operator"].ToString() ?? "").Equals("true", StringComparison.OrdinalIgnoreCase)
        };
    }

    public async Task SaveUserAsync(UserViewModel model)
    {
        bool isNew = string.IsNullOrWhiteSpace(model.UserId);
        string id = model.UserId ?? Guid.NewGuid().ToString();
        const string insertSql = """
            INSERT INTO users (user_id, email, full_name, phone, role, is_manager, is_engineer, is_operator)
            VALUES (@id, @email, @fullName, @phone, @role, @isManager, @isEngineer, @isOperator)
            """;
        const string updateSql = """
            UPDATE users
            SET email = @email, full_name = @fullName, phone = @phone, role = @role,
                is_manager = @isManager, is_engineer = @isEngineer, is_operator = @isOperator
            WHERE user_id = @id
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(isNew ? insertSql : updateSql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@email", model.Email);
        command.Parameters.AddWithValue("@fullName", model.FullName);
        command.Parameters.AddWithValue("@phone", (object?)model.Phone ?? DBNull.Value);
        command.Parameters.AddWithValue("@role", (object?)model.Role ?? DBNull.Value);
        command.Parameters.AddWithValue("@isManager", model.IsManager ? "true" : "false");
        command.Parameters.AddWithValue("@isEngineer", model.IsEngineer ? "true" : "false");
        command.Parameters.AddWithValue("@isOperator", model.IsOperator ? "true" : "false");
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<CompanyViewModel>> GetCompaniesAsync()
    {
        const string query = "SELECT company FROM companies ORDER BY company";
        var result = new List<CompanyViewModel>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new CompanyViewModel
            {
                Company = reader["company"].ToString() ?? string.Empty
            });
        }
        return result;
    }
}
