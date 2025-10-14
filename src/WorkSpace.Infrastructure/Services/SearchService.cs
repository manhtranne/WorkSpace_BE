using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Search;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Services;

public class SearchService : ISearchService
{
    private readonly WorkSpaceContext _ctx;

    public SearchService(WorkSpaceContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<PagedResponse<SearchWorkspacesResponse>> SearchWorkspacesAsync(SearchWorkspacesRequest req)
    {

        var baseQuery = _ctx.Workspaces
            .AsNoTracking()
            .Include(w => w.Address)
            .Include(w => w.WorkspaceAmenities).ThenInclude(wa => wa.Amenity)
            .Include(w => w.WorkspaceImages)
            .Include(w => w.Reviews)
            .Include(w => w.AvailabilitySchedules)
            .Include(w => w.BlockedTimeSlots)
            .Include(w => w.Bookings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Ward))
            baseQuery = baseQuery.Where(w => w.Address.Ward == req.Ward);

        if (req.Participants.HasValue && req.Participants.Value > 0)
            baseQuery = baseQuery.Where(w => w.Capacity >= req.Participants.Value);


        DateTime? startDt = null, endDt = null;
        if (req.Date.HasValue && req.StartTime.HasValue && req.EndTime.HasValue)
        {
            startDt = req.Date.Value.ToDateTime(req.StartTime.Value);
            endDt = req.Date.Value.ToDateTime(req.EndTime.Value);
            var day = req.Date.Value.DayOfWeek;
            var startSpan = req.StartTime.Value.ToTimeSpan();
            var endSpan = req.EndTime.Value.ToTimeSpan();

            baseQuery = baseQuery.Where(w =>
                w.AvailabilitySchedules.Any(a =>
                    a.DayOfWeek == day &&
                    a.IsAvailable &&
                    a.StartTime <= startSpan &&
                    a.EndTime >= endSpan));


            baseQuery = baseQuery.Where(w =>
                !w.BlockedTimeSlots.Any(b => b.StartTime < endDt && b.EndTime > startDt));

            baseQuery = baseQuery.Where(w =>
                !w.Bookings.Any(b => b.StartTimeUtc < endDt && b.EndTimeUtc > startDt));
        }

 
        if (req.PriceMin.HasValue)
            baseQuery = baseQuery.Where(w => w.PricePerHour >= req.PriceMin.Value);
        if (req.PriceMax.HasValue)
            baseQuery = baseQuery.Where(w => w.PricePerHour <= req.PriceMax.Value);

    
        var amenityFacet = await _ctx.WorkspaceAmenities
            .AsNoTracking()
            .Where(wa => baseQuery.Select(w => w.Id).Contains(wa.WorkspaceId))
            .GroupBy(wa => new { wa.AmenityId, wa.Amenity.Name })
            .Select(g => new AmenityFilterOption
            {
                AmenityId = g.Key.AmenityId,
                Name = g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        if (req.AmenityIds != null && req.AmenityIds.Count > 0)
        {
            foreach (var amenityId in req.AmenityIds.Distinct())
            {
                baseQuery = baseQuery.Where(w => w.WorkspaceAmenities.Any(wa => wa.AmenityId == amenityId));
            }
        }

        if (!string.IsNullOrWhiteSpace(req.QueryText))
        {
            var q = req.QueryText.Trim();
            var pattern = $"%{q}%";

            baseQuery = baseQuery.Where(w =>
                EF.Functions.Like(EF.Functions.Collate(w.Title, "SQL_Latin1_General_CP1_CI_AI"), pattern) ||
                EF.Functions.Like(EF.Functions.Collate(w.Description, "SQL_Latin1_General_CP1_CI_AI"), pattern));
        }


        baseQuery = req.SortBy?.ToLowerInvariant() switch
        {
            "rating" => baseQuery
                .OrderByDescending(w => w.Reviews.Any() ? w.Reviews.Average(r => r.Rating) : 0.0)
                .ThenByDescending(w => w.Reviews.Count),
            "price" => (req.SortDesc ? baseQuery.OrderByDescending(w => w.PricePerHour) : baseQuery.OrderBy(w => w.PricePerHour)),
            "newest" => baseQuery.OrderByDescending(w => w.CreatedAt),
            "relevance" when !string.IsNullOrWhiteSpace(req.QueryText) =>
        baseQuery
            .OrderByDescending(w => EF.Functions.Like(EF.Functions.Collate(w.Title, "SQL_Latin1_General_CP1_CI_AI"), $"{req.QueryText}%"))
            .ThenByDescending(w => EF.Functions.Like(EF.Functions.Collate(w.Title, "SQL_Latin1_General_CP1_CI_AI"), $"% {req.QueryText}%"))
            .ThenByDescending(w => EF.Functions.Like(EF.Functions.Collate(w.Title, "SQL_Latin1_General_CP1_CI_AI"), $"%{req.QueryText}%"))
            .ThenByDescending(w => EF.Functions.Like(EF.Functions.Collate(w.Description, "SQL_Latin1_General_CP1_CI_AI"), $"%{req.QueryText}%"))
            .ThenByDescending(w => w.IsFeatured),

            _ => baseQuery.OrderByDescending(w => w.IsFeatured)
                          .ThenByDescending(w => w.Reviews.Any() ? w.Reviews.Average(r => r.Rating) : 0.0)
        };

        var total = await baseQuery.CountAsync();


        var skip = (req.PageNumber - 1) * req.PageSize;
        var pageItems = await baseQuery
            .Skip(skip)
            .Take(req.PageSize)
            .Select(w => new SearchWorkspaceDto
            {
                Id = w.Id,
                Title = w.Title,
                ShortDescription = w.Description,
                PricePerHour = w.PricePerHour,
                Capacity = w.Capacity,
                Area = w.Area,
                AddressText = w.Address != null ? $"{w.Address.Street}, {w.Address.Ward}, {w.Address.District}, {w.Address.City}" : null,
                Ward = w.Address.Ward,
                District = w.Address.District,
                City = w.Address.City,
                ThumbnailUrl = w.WorkspaceImages.OrderBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault(),
                AverageRating = w.Reviews.Any() ? w.Reviews.Average(r => r.Rating) : 0.0,
                ReviewCount = w.Reviews.Count,
                AmenityNames = w.WorkspaceAmenities.Select(wa => wa.Amenity.Name).ToList()
            })
            .ToListAsync();

        var priceStats = await _ctx.Workspaces
            .AsNoTracking()
            .Where(w => baseQuery.Select(x => x.Id).Contains(w.Id))
            .Select(w => w.PricePerHour)
            .ToListAsync();

        decimal? minPrice = priceStats.Count > 0 ? priceStats.Min() : null;
        decimal? maxPrice = priceStats.Count > 0 ? priceStats.Max() : null;

        try
        {
            var amenityCsv = (req.AmenityIds != null && req.AmenityIds.Count > 0) ? string.Join(",", req.AmenityIds.Distinct()) : null;
            _ctx.SearchQueryHistories.Add(new SearchQueryHistory
            {
                UserId = null,
                Ward = req.Ward,
                Date = req.Date,
                StartTime = req.StartTime,
                EndTime = req.EndTime,
                Participants = req.Participants,
                PriceMin = req.PriceMin,
                PriceMax = req.PriceMax,
                AmenityIdsCsv = amenityCsv,
                ResultsCount = total,
                ClientIp = null,
                UserAgent = null,
                CreatedAt = DateTime.UtcNow,
                QueryText = req.QueryText,
            });
            await _ctx.SaveChangesAsync();
        }
        catch { /* best-effort logging, tránh chặn response */ }

        var resp = new SearchWorkspacesResponse
        {
            Items = pageItems,
            Total = total,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            AmenityOptions = amenityFacet
        };

        return new PagedResponse<SearchWorkspacesResponse>(resp, req.PageNumber, req.PageSize);
    }

    public async Task<PagedResponse<List<SearchQueryHistory>>> GetSearchHistoryAsync(int pageNumber = 1, int pageSize = 50, int? userId = null)
    {
        var query = _ctx.SearchQueryHistories.AsNoTracking().OrderByDescending(x => x.CreatedAt).AsQueryable();
        if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);

        var total = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResponse<List<SearchQueryHistory>>(items, pageNumber, pageSize);
    }
}


