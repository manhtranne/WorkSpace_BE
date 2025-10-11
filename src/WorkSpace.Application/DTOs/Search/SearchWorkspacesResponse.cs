using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSpace.Application.DTOs.Search;

public class SearchWorkspacesResponse
{
    public List<SearchWorkspaceDto> Items { get; set; }
    public int Total { get; set; }

    // Gợi ý filter (facet)
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<AmenityFilterOption> AmenityOptions { get; set; }
}
