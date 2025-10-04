using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using WorkSpace.Domain.Entities;
using WorkSpace.Infrastructure;
using WorkSpace.WebApi.Middlewares;

namespace WorkSpace.WebApi.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddPresentation(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<WorkSpaceContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("WorkSpace.Infrastructure") // nếu migrations nằm ở Infrastructure
            )
        );
        builder.Services
            .AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<WorkSpaceContext>()  
            .AddDefaultTokenProviders();
        builder.Services.AddAuthentication();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme()
            {
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearerAuth" }
                    },
                    []
                }
            });
        });

        
        builder.Host.UseSerilog((context, configuration) =>
            configuration
                .ReadFrom.Configuration(context.Configuration)
        );
    }
}