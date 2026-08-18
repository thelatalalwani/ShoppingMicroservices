using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly OrderRepository _orderRepository;

    public OrdersController(HttpClient httpClient, OrderRepository orderRepository)
    {
        _httpClient = httpClient;
        _orderRepository = orderRepository;
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

        order.TotalAmount = product.Price * order.Quantity;
        order.Id = await _orderRepository.CreateAsync(order);

        return Ok(new
        {
            message = "Order created successfully",
            order,
            productName = product.Name
        });
    }
}
