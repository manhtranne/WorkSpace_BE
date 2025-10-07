using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Enums;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Seeds;

public class WorkSpaceSeeder(WorkSpaceContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : IWorkSpaceSeeder
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (!await context.Database.CanConnectAsync()) return;
        await context.Database.MigrateAsync(ct);

        if (await roleManager.Roles.AnyAsync(ct)) return;

        var rolesToSeed = new (string Name, string DisplayName)[]
        {
            (Roles.SuperAdmin.ToString(), "Super Administrator"),
            (Roles.Admin.ToString(), "Admin"),
            (Roles.Moderator.ToString(), "Moderator"),
            (Roles.Basic.ToString(), "Basic User")
        };
        
        foreach (var (name, displayName) in rolesToSeed)
        {
            await CreateRoleIfNotExistsAsync(name, displayName,ct);
        }
        
        const string adminEmail = "superadmin@workspace.local";
        const string adminPass  = "Password1!";
        await EnsureUserWithRolesAsync(
            email: adminEmail,
            password: adminPass,
            firstName: "Super",
            lastName: "Admin",
            roles: new[] { Roles.SuperAdmin.ToString() },
            ct: ct
        );

    }
    
   
private async Task CreateRoleIfNotExistsAsync(string roleName, string displayName, CancellationToken ct)
    {
        if (await roleManager.RoleExistsAsync(roleName)) return;

        var role = new AppRole
        {
            Name = roleName,
            DisplayName = displayName
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
            throw new InvalidOperationException($"Create role '{roleName}' failed: {errors}");
        }
    }

    private async Task EnsureUserWithRolesAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        IEnumerable<string> roles,
        CancellationToken ct = default
    )
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                DateCreated = DateTime.UtcNow
            };

            var createRes = await userManager.CreateAsync(user, password);
            if (!createRes.Succeeded)
            {
                var errors = string.Join("; ", createRes.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Create user '{email}' failed: {errors}");
            }
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var toAdd = roles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToArray();
        if (toAdd.Length > 0)
        {
            var addRes = await userManager.AddToRolesAsync(user, toAdd);
            if (!addRes.Succeeded)
            {
                var errors = string.Join("; ", addRes.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Add roles '{string.Join(",", toAdd)}' to '{email}' failed: {errors}");
            }
        }
    }
}