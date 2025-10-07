using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface IHostProfileAsyncRepository  : IGenericRepositoryAsync<Domain.Entities.HostProfile>
{
    public Task<HostProfile?> GetHostProfileByUserId(int userId);
}