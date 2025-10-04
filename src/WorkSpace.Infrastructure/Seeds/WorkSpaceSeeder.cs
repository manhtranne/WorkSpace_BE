using Microsoft.AspNetCore.Identity;
using WorkSpace.Application.Enums;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Seeds;

public class WorkSpaceSeeder(WorkSpaceContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : IWorkSpaceSeeder
{
    public async Task SeedAsync()
    {
        // if (!await context.database.canconnectasync())
        //     return;
        // await ensureroleasync(roles.superadmin.tostring(), "super administrator");
        // await ensureroleasync(roles.admin.tostring(),       "administrator");
        // await ensureroleasync(roles.moderator.tostring(),   "moderator");
        // await ensureroleasync(roles.basic.tostring(),       "basic user");
        //
        // const string adminemail = "superadmin@workspace.local";
        // const string adminpass  = "p@ssw0rd!";
        // await ensuresuperadminasync(adminemail, adminpass, roles.superadmin.tostring());

    }
    
    private async Task EnsureRoleAsync(string roleName, string displayName)
    {

        var exists = await roleManager.RoleExistsAsync(roleName);
        if (exists) return;

        var role = new AppRole
        {
            Name = roleName,
            DisplayName = displayName,
            NormalizedName = roleName.ToUpperInvariant()
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
            throw new InvalidOperationException($"Create role '{roleName}' failed: {errors}");
        }
    }

    private async Task EnsureSuperAdminAsync(string email, string password, string roleName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            if (roleName == Roles.SuperAdmin.ToString())
            {
                user = new AppUser()
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = "Super",
                    LastName = "Admin"
                };
            }

            if (roleName == Roles.Admin.ToString())
            {
                user = new AppUser()
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = "",
                    LastName = "Admin"
                };
            }
            if (roleName == Roles.Moderator.ToString())
            {
                user = new AppUser()
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = "",
                    LastName = "Moderator"
                };
            }
            if (roleName == Roles.Basic.ToString())
            {
                user = new AppUser()
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = "",
                    LastName = "Basic"
                };
            }


            var createRes = await userManager.CreateAsync(user, password);
            if (!createRes.Succeeded)
            {
                var errors = string.Join("; ", createRes.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Create super admin failed: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var addRes = await userManager.AddToRoleAsync(user, roleName);
            if (!addRes.Succeeded)
            {
                var errors = string.Join("; ", addRes.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Add role '{roleName}' to '{email}' failed: {errors}");
            }
        }
    }
}