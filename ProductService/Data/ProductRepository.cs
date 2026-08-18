using Microsoft.Data.SqlClient;
using ProductService.Models;

namespace ProductService.Data;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ProductDB")!;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        var products = new List<Product>();

        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand(
            "SELECT Id, Name, Price FROM Products",
            connection);

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2)
            });
        }

        return products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        var command = new SqlCommand(
            "SELECT Id, Name, Price FROM Products WHERE Id = @Id",
            connection);

        command.Parameters.AddWithValue("@Id", id);

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2)
            };
        }

        return null;
    }
}
