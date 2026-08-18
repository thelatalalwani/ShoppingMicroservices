using Microsoft.Data.SqlClient;
using OrderService.Models;

namespace OrderService.Data;

public class OrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("OrderDB")!;
    }

    public async Task<int> CreateAsync(Order order)
    {
        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand(
            @"INSERT INTO Orders (ProductId, Quantity, TotalAmount)
              OUTPUT INSERTED.Id
              VALUES (@ProductId, @Quantity, @TotalAmount)",
            connection);

        command.Parameters.AddWithValue("@ProductId", order.ProductId);
        command.Parameters.AddWithValue("@Quantity", order.Quantity);
        command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);

        await connection.OpenAsync();
        var id = (int)await command.ExecuteScalarAsync();
        return id;
    }
}
