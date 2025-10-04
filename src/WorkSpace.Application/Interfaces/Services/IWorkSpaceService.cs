namespace WorkSpace.Application.Interfaces.Services;

public interface IWorkSpaceService
{
    Task AddUserToWorkSpaceAsync(string workSpaceId, string userId);
}