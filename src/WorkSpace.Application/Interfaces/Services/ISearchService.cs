using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WorkSpace.Application.DTOs.Search;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Services;

public interface ISearchService
{
    Task<PagedResponse<SearchWorkspacesResponse>> SearchWorkspacesAsync(SearchWorkspacesRequest request);
    Task<PagedResponse<List<SearchQueryHistory>>> GetSearchHistoryAsync(int pageNumber = 1, int pageSize = 50, int? userId = null);
}
