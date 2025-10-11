using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Lookup;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Infrastructure.Services;

public class LookupService : ILookupService
{
    private readonly WorkSpaceContext _ctx;
    public LookupService(WorkSpaceContext ctx) { _ctx = ctx; }

    public async Task<Response<List<WardDto>>> GetAllWardsAsync(string? q = null, int? take = null)
    {
        var query = _ctx.Addresses.AsNoTracking()
            .Where(a => a.Ward != null && a.Ward != "")
            .Select(a => new WardDto
            {
                Ward = a.Ward,
                District = a.District,
                City = a.City
            })
            .Distinct();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.Trim();
            query = query.Where(x => x.Ward.Contains(k) || x.District.Contains(k) || x.City.Contains(k));
        }

        if (take.HasValue && take.Value > 0) query = query.Take(take.Value);

        var data = await query.OrderBy(x => x.City).ThenBy(x => x.District).ThenBy(x => x.Ward).ToListAsync();
        return new Response<List<WardDto>>(data);
    }
}
