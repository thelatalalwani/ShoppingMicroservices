using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using OrderService.Messaging;
using OrderService.Models;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly OrderRepository _orderRepository;
    private readonly RabbitMqPublisher _rabbitMqPublisher;

    public OrdersController(
        HttpClient httpClient,
        OrderRepository orderRepository,
        RabbitMqPublisher rabbitMqPublisher)
    {
        _httpClient = httpClient;
        _orderRepository = orderRepository;
        _rabbitMqPublisher = rabbitMqPublisher;
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

        await _rabbitMqPublisher.PublishOrderCreatedAsync(new
        {
            OrderId = order.Id,
            ProductId = order.ProductId,
            TotalAmount = order.TotalAmount
        });

        return Ok(new
        {
            message = "Order created successfully",
            order,
            productName = product.Name
        });
    }
}
