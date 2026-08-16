using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public OrdersController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(Order order)
    {
        var product = await _httpClient.GetFromJsonAsync<ProductDto>(
            $"http://localhost:5166/api/products/{order.ProductId}");

        if (product == null)
        {
            return BadRequest("Product not found");
        }

        var totalAmount = product.Price * order.Quantity;

        return Ok(new
        {
            message = "Order created successfully",
            order,
            productName = product.Name,
            totalAmount
        });
    }
}
