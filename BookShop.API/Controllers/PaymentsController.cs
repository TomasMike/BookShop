using Microsoft.AspNetCore.Mvc;
using BookShop.Core.Services;
using BookShop.Core.Models;
using BookShop.API.DTOs;

namespace BookShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Processes a payment for an order
        /// </summary>
        /// <param name="request">Payment processing request</param>
        /// <returns>The created payment</returns>
        [HttpPost]
        public async Task<ActionResult<PaymentDto>> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
                    return BadRequest(new { error = "Invalid payment method" });

                var payment = await _paymentService.ProcessPaymentAsync(
                    request.OrderId,
                    request.Amount,
                    paymentMethod);

                var dto = MapToPaymentDto(payment);
                return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets a specific payment by ID
        /// </summary>
        /// <param name="id">The payment ID</param>
        /// <returns>The payment details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound(new { error = "Payment not found" });

            var dto = MapToPaymentDto(payment);
            return Ok(dto);
        }

        /// <summary>
        /// Gets all payments for a specific order
        /// </summary>
        /// <param name="orderId">The order ID</param>
        /// <returns>List of order's payments</returns>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetOrderPayments(int orderId)
        {
            var payments = await _paymentService.GetPaymentsByOrderAsync(orderId);
            var dtos = payments.Select(MapToPaymentDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Refunds a completed payment
        /// </summary>
        /// <param name="id">The payment ID</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/refund")]
        public async Task<ActionResult> RefundPayment(int id)
        {
            try
            {
                var success = await _paymentService.RefundPaymentAsync(id);
                if (!success)
                    return NotFound(new { error = "Payment not found" });

                return Ok(new { message = "Payment refunded successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private static PaymentDto MapToPaymentDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod.ToString(),
                Status = payment.Status.ToString(),
                TransactionId = payment.TransactionId
            };
        }
    }
}
