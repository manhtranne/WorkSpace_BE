using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Search;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Infrastructure.Services;

public class WorkSpaceService : IWorkSpaceService
{
    private readonly WorkSpaceContext _ctx;
    public WorkSpaceService(WorkSpaceContext ctx) { _ctx = ctx; }

    public Task AddUserToWorkSpaceAsync(string workSpaceId, string userId)
    {
   
        return Task.CompletedTask;
    }

    public async Task<Response<List<SearchWorkspaceDto>>> GetFeaturedAsync(int take = 4)
    {
        var q = _ctx.Workspaces.AsNoTracking()
            .Include(w => w.Address)
            .Include(w => w.WorkspaceImages)
            .Include(w => w.Reviews)
            .OrderByDescending(w => w.IsFeatured)
            .ThenByDescending(w => w.Reviews.Any() ? w.Reviews.Average(r => r.Rating) : 0.0)
            .ThenByDescending(w => w.Reviews.Count)
            .Take(take);

        var data = await q.Select(w => new SearchWorkspaceDto
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
            AmenityNames = new List<string>() 
        }).ToListAsync();

        return new Response<List<SearchWorkspaceDto>>(data);
    }
}
