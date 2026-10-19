using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookShop.Core.Services;
using BookShop.API.DTOs;

namespace BookShop.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;

        public OrdersController(IOrderService orderService, ICustomerService customerService)
        {
            _orderService = orderService;
            _customerService = customerService;
        }

        /// <summary>
        /// Adds a book to the customer's latest open order or creates an order.
        /// </summary>
        /// <param name="request">Order creation request containing the book ID</param>
        /// <returns>The updated or newly created order</returns>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (userId == null)
                return Unauthorized();

            var customer = await _customerService.GetCustomerByUserIdAsync(userId);
            if (customer == null)
                return BadRequest(new { error = "No customer profile is associated with this account" });

            try
            {
                var (order, created) = await _orderService.AddBookToOpenOrderAsync(request.BookId, customer.Id);
                var dto = MapToOrderDto(order);
                return created
                    ? CreatedAtAction(nameof(GetOrder), new { id = order.Id }, dto)
                    : Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific order by ID
        /// </summary>
        /// <param name="id">The order ID</param>
        /// <returns>The order details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            var customer = await GetCurrentCustomerAsync();
            if (customer == null)
                return NotFound(new { error = "No customer profile is associated with this account" });
            if (order == null || order.CustomerId != customer.Id)
                return NotFound(new { error = "Order not found" });

            var dto = MapToOrderDto(order);
            return Ok(dto);
        }

        /// <summary>
        /// Gets all orders for a specific customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <returns>List of customer's orders</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrders(int customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerAsync(customerId);
            var dtos = orders.Select(MapToOrderDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Confirms a pending order
        /// </summary>
        /// <param name="id">The order ID</param>
        /// <returns>The confirmed order</returns>
        [HttpPost("{id}/confirm")]
        public async Task<ActionResult<OrderDto>> ConfirmOrder(int id)
        {
            try
            {
                var order = await _orderService.ConfirmOrderAsync(id);
                var dto = MapToOrderDto(order);
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancels an order
        /// </summary>
        /// <param name="id">The order ID</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelOrder(int id)
        {
            try
            {
                var success = await _orderService.CancelOrderAsync(id);
                if (!success)
                    return NotFound(new { error = "Order not found" });

                return Ok(new { message = "Order cancelled successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("customer")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (userId == null)
                return Unauthorized();

            var customer = await _customerService.GetCustomerByUserIdAsync(userId);
            if (customer == null)
                return NotFound(new { error = "No customer profile is associated with this account" });

            var orders = await _orderService.GetOrdersByCustomerAsync(customer.Id);
            return Ok(orders.Select(MapToOrderDto).ToList());
        }

        [HttpPut("{orderId}/details/{detailId}")]
        public async Task<ActionResult<OrderDto>> UpdateOrderDetail(int orderId, int detailId, [FromBody] UpdateOrderDetailRequest request)
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null)
                return NotFound(new { error = "No customer profile is associated with this account" });

            try
            {
                var order = await _orderService.UpdateOrderDetailQuantityAsync(orderId, detailId, customer.Id, request.Quantity);
                return order == null ? NotFound(new { error = "Order detail not found" }) : Ok(MapToOrderDto(order));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{orderId}/details/{detailId}")]
        public async Task<ActionResult<OrderDto>> RemoveOrderDetail(int orderId, int detailId)
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null)
                return NotFound(new { error = "No customer profile is associated with this account" });

            try
            {
                var order = await _orderService.RemoveOrderDetailAsync(orderId, detailId, customer.Id);
                return order == null ? NotFound(new { error = "Order detail not found" }) : Ok(MapToOrderDto(order));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private async Task<BookShop.Core.Models.Customer?> GetCurrentCustomerAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return userId == null ? null : await _customerService.GetCustomerByUserIdAsync(userId);
        }

        private static OrderDto MapToOrderDto(BookShop.Core.Models.Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                TotalPrice = order.TotalPrice,
                Details = order.Details.Select(detail => new OrderDetailDto
                {
                    Id = detail.Id,
                    BookId = detail.BookId,
                    BookTitle = detail.Book.Title,
                    AuthorName = detail.Book.Author.Name,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    LineTotal = detail.UnitPrice * detail.Quantity
                }).ToList()
            };
        }
    }
}
