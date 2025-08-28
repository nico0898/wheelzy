using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.DTOs;

namespace api.Interfaces;

public interface ICarQueryService
{
    Task<List<CarSummaryDTO>> GetSummaryEfAsync();
    Task<List<CarSummaryDTO>> GetSummarySpAsync();

    Task ChangeStatusAsync(int carId, int statusId, DateTime? statusDate, string? changedBy);

    Task<List<OrderDTO>> GetOrders(DateTime? dateFrom, DateTime? dateTo, List<int>? customerIds, List<int>? statusIds, bool? isActive);
    Task<OfferCreatedDTO> CreateAndMakeCurrentAsync(int carId, int buyerId, decimal amount);
}
