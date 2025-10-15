using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Interfaces.Services
{
    public interface ISearchService
    {
        Task<PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>> SearchWorkSpaceRoomsAsync(SearchRequestDto request);
        Task<IEnumerable<string>> GetLocationSuggestionsAsync(string query);
    }
}