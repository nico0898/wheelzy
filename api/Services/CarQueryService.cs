using api.DTOs;
using api.Entities;
using api.Interfaces;
using api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class CarQueryService(AppDbContext db) : ICarQueryService
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarSummaryDTO>> GetSummaryEfAsync()
    {
        var query =
            from c in _db.Cars
            join cs in _db.CarSpecs on c.CarSpecId equals cs.Id
            join mk in _db.Makes on cs.MakeId equals mk.Id
            join md in _db.Models on cs.ModelId equals md.Id
            join sm0 in _db.SubModels on cs.SubModelId equals sm0.Id into smg
            from sm in smg.DefaultIfEmpty()
                // Oferta actual
            join o0 in _db.Offers on c.Id equals o0.CarId into og
            from o in og.Where(x => x.Current).DefaultIfEmpty()
            join b0 in _db.Buyers on o.BuyerId equals b0.Id into bg
            from b in bg.DefaultIfEmpty()
                // Estado actual
            join s0 in _db.CarStatuses on c.Id equals s0.CarId into sg
            from s in sg.Where(x => x.IsCurrent).DefaultIfEmpty()
            join st0 in _db.Statuses on s.StatusId equals st0.Id into stg
            from st in stg.DefaultIfEmpty()

            select new CarSummaryDTO
            {
                CarId = c.Id,
                CarYear = cs.Year,
                Make = mk.Name,
                Model = md.Name,
                SubModel = sm != null ? sm.Name : null,
                CurrentBuyer = b != null ? b.Name : null,
                CurrentQuote = o != null ? o.Amount : (decimal?)null,
                CurrentStatus = st != null ? st.Name : null,
                CurrentStatusDate = s != null ? s.StatusDate : (DateTime?)null
            };

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<List<CarSummaryDTO>> GetSummarySpAsync()
    {
        // Llama al procedimiento almacenado y mapea a CarSummaryRow (keyless)
        var rows = await _db.CarSummaryRows
            .FromSqlRaw("EXEC dbo.usp_CarSummary")
            .AsNoTracking()
            .ToListAsync();

        // Si querés mantener el DTO separado, proyectamos aquí (1:1)
        return rows.Select(r => new CarSummaryDTO
        {
            CarId = r.CarId,
            CarYear = r.CarYear,
            Make = r.Make,
            Model = r.Model,
            SubModel = r.SubModel,
            CurrentBuyer = r.CurrentBuyer,
            CurrentQuote = r.CurrentQuote,
            CurrentStatus = r.CurrentStatus,
            CurrentStatusDate = r.CurrentStatusDate
        }).ToList();
    }

    public async Task ChangeStatusAsync(int carId, int statusId, DateTime? statusDate, string? changedBy)
    {
        // Validaciones
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new Exception($"Car {carId} no existe");

        var status = await _db.Statuses
            .Where(s => s.Id == statusId)
            .Select(s => new { s.Id, s.RequiresStatusDate, s.Name })
            .FirstOrDefaultAsync();

        if (status is null) throw new KeyNotFoundException($"Status {statusId} not found.");

        if (status.RequiresStatusDate && statusDate is null)
            throw new ArgumentException($"El estado '{status.Name}' requiere StatusDate.");

        // 1) Desmarcar estado actual (si hubiera)
        await _db.CarStatuses
            .Where(cs => cs.CarId == carId && cs.IsCurrent)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsCurrent, false));

        // 2) Insertar nuevo estado como actual
        var newStatus = new CarStatus
        {
            CarId = carId,
            StatusId = statusId,
            StatusDate = statusDate,             // puede ser null si no es requerido
            ChangedBy = string.IsNullOrWhiteSpace(changedBy) ? "system" : changedBy,
            ChangedAt = DateTime.Now,
            IsCurrent = true
        };

        _db.CarStatuses.Add(newStatus);
        await _db.SaveChangesAsync();
    }

    public async Task<List<OrderDTO>> GetOrders(DateTime? dateFrom, DateTime? dateTo, List<int>? customerIds, List<int>? statusIds, bool? isActive)
    {
        var q =
            from o in _db.Offers
            join b in _db.Buyers on o.BuyerId equals b.Id
            // estado ACTUAL del auto (para filtrar por statusIds)
            join cs0 in _db.CarStatuses on o.CarId equals cs0.CarId into csg
            from cs in csg.Where(x => x.IsCurrent).DefaultIfEmpty()
            join st0 in _db.Statuses on cs.StatusId equals st0.Id into stg
            from st in stg.DefaultIfEmpty()
            select new { o, b, cs, st };

        if (dateFrom.HasValue)
            q = q.Where(x => x.o.Date >= dateFrom.Value);

        if (dateTo.HasValue)
            q = q.Where(x => x.o.Date < dateTo.Value); // límite superior exclusivo

        if (customerIds != null && customerIds.Count > 0)
            q = q.Where(x => customerIds.Contains(x.b.Id));

        if (statusIds != null && statusIds.Count > 0)
            q = q.Where(x => x.cs != null && statusIds.Contains(x.cs.StatusId));

        if (isActive.HasValue)
            q = q.Where(x => x.o.Current == isActive.Value);

        return await q
            .AsNoTracking()
            .Select(x => new OrderDTO
            {
                OfferId = x.o.Id,
                CarId = x.o.CarId,
                BuyerId = x.b.Id,
                BuyerName = x.b.Name,
                Amount = x.o.Amount,
                Date = x.o.Date,
                IsActive = x.o.Current,
                StatusId = x.cs != null ? x.cs.StatusId : (int?)null,
                StatusName = x.st != null ? x.st.Name : null
            })
            .ToListAsync();
    }

    public async Task<OfferCreatedDTO> CreateAndMakeCurrentAsync(int carId, int buyerId, decimal amount)
    {
        // Validaciones simples
        if (amount <= 0) throw new Exception("Amount debe ser > 0.");

        var car = await _db.Cars.FindAsync(carId);
        if (car == null) throw new Exception($"Car {carId} no existe.");

        var buyer = await _db.Buyers.FindAsync(buyerId);
        if (buyer == null) throw new Exception($"Buyer {buyerId} no existe.");

        // 1) Crear la oferta (arranca como no actual para no chocar con la actual existente)
        var offer = new Offer
        {
            CarId  = carId,
            BuyerId= buyerId,
            Amount = amount,
            Current= false,
            Date   = DateTime.Now
        };

        _db.Offers.Add(offer);
        await _db.SaveChangesAsync(); // obtenemos Id

        // 2) Desmarcar cualquier otra “Current” del mismo auto
        var previousCurrents = await _db.Offers
            .Where(o => o.CarId == carId && o.Current)
            .ToListAsync();

        foreach (var o in previousCurrents)
            o.Current = false;

        // 3) Marcar la nueva como “Current” (regla: la última creada es la actual)
        offer.Current = true;

        await _db.SaveChangesAsync();

        return new OfferCreatedDTO
        {
            OfferId = offer.Id,
            CarId   = carId,
            BuyerId = buyerId,
            Amount  = amount,
            Date    = offer.Date,
            IsCurrent = true
        };
    }

}
