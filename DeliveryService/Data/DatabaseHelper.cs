using System;
using System.Collections.Generic;
using Npgsql;
using DeliveryService.Models;

namespace DeliveryService.Data
{
    public class DatabaseHelper
    {
        // Строка подключения к PostgreSQL
        private const string ConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=1234";

        // Инициализация структуры БД и начальных данных
        public void InitializeDatabase()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Couriers (
                    Id SERIAL PRIMARY KEY,
                    FullName TEXT NOT NULL,
                    Transport TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS Orders (
                    Id SERIAL PRIMARY KEY,
                    Address TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    CourierId INTEGER,
                    FOREIGN KEY(CourierId) REFERENCES Couriers(Id) ON DELETE SET NULL
                );";
            command.ExecuteNonQuery();

            command.CommandText = "SELECT COUNT(*) FROM Couriers";
            if (Convert.ToInt64(command.ExecuteScalar()) == 0)
            {
                command.CommandText = "INSERT INTO Couriers (FullName, Transport) VALUES ('Иван Иванов', 'Велосипед'), ('Петр Петров', 'Авто');";
                command.ExecuteNonQuery();
                command.CommandText = "INSERT INTO Orders (Address, Status, CourierId) VALUES ('ул. Ленина 1', 'Доставлен', 1), ('ул. Пушкина 2', 'Ожидает', 2);";
                command.ExecuteNonQuery();
            }
        }

        // Получение заказов с пагинацией, сортировкой и поиском (п.1, п.6, п.9 ТЗ)
        public List<OrderDisplay> GetOrders(int pageNumber, int pageSize, string sortBy, string search)
        {
            var list = new List<OrderDisplay>();
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            int offset = (pageNumber - 1) * pageSize;
            
            // Безопасный выбор столбца сортировки
            string sortColumn = sortBy == "Address" ? "Orders.Address" : 
                                sortBy == "Courier" ? "Couriers.FullName" : "Orders.Id";

            string query = $@"
                SELECT Orders.Id, Orders.Address, Orders.Status, Couriers.Id, Couriers.FullName, Couriers.Transport
                FROM Orders
                LEFT JOIN Couriers ON Orders.CourierId = Couriers.Id
                WHERE Orders.Address ILIKE @Search OR Couriers.FullName ILIKE @Search OR Orders.Status ILIKE @Search
                ORDER BY {sortColumn}
                LIMIT @Limit OFFSET @Offset";
                
            command.CommandText = query;
            command.Parameters.AddWithValue("@Search", $"%{search}%");
            command.Parameters.AddWithValue("@Limit", pageSize);
            command.Parameters.AddWithValue("@Offset", offset);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new OrderDisplay
                {
                    OrderId = reader.GetInt32(0),
                    Address = reader.GetString(1),
                    Status = reader.GetString(2),
                    CourierId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    CourierName = reader.IsDBNull(4) ? "Не назначен" : reader.GetString(4),
                    CourierTransport = reader.IsDBNull(5) ? "-" : reader.GetString(5)
                });
            }
            return list;
        }

        // Получение общего количества для пагинации
        public int GetTotalOrdersCount(string search)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) FROM Orders
                LEFT JOIN Couriers ON Orders.CourierId = Couriers.Id
                WHERE Orders.Address ILIKE @Search OR Couriers.FullName ILIKE @Search OR Orders.Status ILIKE @Search";
            command.Parameters.AddWithValue("@Search", $"%{search}%");
            return Convert.ToInt32(command.ExecuteScalar());
        }

        // Поиск или немедленное добавление курьера (п.7 ТЗ)
        public int GetOrCreateCourier(string fullName, string transport)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            
            command.CommandText = "SELECT Id FROM Couriers WHERE FullName = @FullName";
            command.Parameters.AddWithValue("@FullName", fullName);
            var result = command.ExecuteScalar();
            if (result != null) return Convert.ToInt32(result);

            command.CommandText = "INSERT INTO Couriers (FullName, Transport) VALUES (@FullName, @Transport) RETURNING Id;";
            command.Parameters.AddWithValue("@Transport", string.IsNullOrEmpty(transport) ? "Пешком" : transport);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public void AddOrder(string address, string status, int courierId)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Orders (Address, Status, CourierId) VALUES (@Address, @Status, @CourierId)";
            command.Parameters.AddWithValue("@Address", address);
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@CourierId", courierId);
            command.ExecuteNonQuery();
        }

        public void UpdateOrder(int orderId, string address, string status, int courierId)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Orders SET Address = @Address, Status = @Status, CourierId = @CourierId WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", orderId);
            command.Parameters.AddWithValue("@Address", address);
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@CourierId", courierId);
            command.ExecuteNonQuery();
        }

        public void DeleteOrder(int orderId)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Orders WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", orderId);
            command.ExecuteNonQuery();
        }
    }
}