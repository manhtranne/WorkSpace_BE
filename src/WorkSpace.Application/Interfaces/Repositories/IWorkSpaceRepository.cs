<<<<<<< Updated upstream
﻿using WorkSpace.Domain.Entities;
=======
﻿
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Domain.Entities;
>>>>>>> Stashed changes

namespace WorkSpace.Application.Interfaces.Repositories
{
<<<<<<< Updated upstream
=======
    public interface IWorkSpaceRepository : IGenericRepositoryAsync<Domain.Entities.WorkSpace>
    {
        Task<WorkSpace.Domain.Entities.WorkSpace?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<WorkSpaceRoom> Rooms, int TotalCount)> GetRoomsPagedAsync(
            WorkSpaceFilter filter,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default
        );

        Task<bool> ExistsTitleForHostAsync(int hostId, string title, CancellationToken cancellationToken = default);

        Task<WorkSpaceRoom?> GetRoomByIdWithDetailsAsync(int roomId, CancellationToken cancellationToken = default);
    }
>>>>>>> Stashed changes
}