using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController(ICarQueryService service) : ControllerBase
    {
        private readonly ICarQueryService _service = service;

        // GET: /api/cars/summary (EF LINQ)
        [HttpGet("summary")]
        public async Task<ActionResult<List<CarSummaryDTO>>> GetSummaryEf() => Ok(await _service.GetSummaryEfAsync());

        // GET: /api/cars/summary-sp (Stored Procedure)
        [HttpGet("summary-sp")]
        public async Task<ActionResult<List<CarSummaryDTO>>> GetSummarySp() => Ok(await _service.GetSummarySpAsync());

        // Cambiar el estado actual del auto (valida fecha si el estado lo requiere)
        // POST /api/cars/{carId}/status/change
        [HttpPost("ChangeStatus")]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusRequest req)
        {
            try
            {
                await _service.ChangeStatusAsync(req.CarId, req.StatusId, req.StatusDate, req.ChangedBy);
                return Ok();
            }
            catch (Exception ex)
            {
                // Log(ex)
                return StatusCode(500, new { message = "Error inesperado.", detail = ex.Message });
            }
        }

        // GET /api/orders?dateFrom=2025-01-01&dateTo=2025-12-31&customerIds=1&customerIds=3&statusIds=2&isActive=true
        [HttpGet]
        public async Task<ActionResult<List<OrderDTO>>> Get(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] List<int>? customerIds,
            [FromQuery] List<int>? statusIds,
            [FromQuery] bool? isActive)
        {
            try
            {
                var data = await _service.GetOrders(dateFrom, dateTo, customerIds, statusIds, isActive);
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Log(ex)
                return StatusCode(500, new { message = "Error inesperado.", detail = ex.Message });
            }
        }

         // POST /api/cars/{carId}/offers
        [HttpPost("CreateOffer")]
        public async Task<ActionResult<OfferCreatedDTO>> CreateOffer([FromBody] CreateOfferRequest req)
        {
            try
            {
                var result = await _service.CreateAndMakeCurrentAsync(req.CarId, req.BuyerId, req.Amount);
                return CreatedAtAction(nameof(CreateOffer), new { id = result.OfferId }, result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Error inesperado al crear la oferta." });
            }
        }
    }
}

public sealed class ChangeStatusRequest
{
    public int CarId { get; set; }
    public int StatusId { get; set; }
    public DateTime? StatusDate { get; set; } // obligatorio si el status lo requiere
    public string? ChangedBy { get; set; }
}

public sealed class CreateOfferRequest
{
    public int CarId { get; set; }
    public int BuyerId { get; set; }
    public decimal Amount { get; set; }
}