using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StoreDataAccess;
using StoreDataAccess.Repository;
using StoreDataAccess.Repository.IRepository;
using StoreModels;

namespace StoreAPIs.StartupConfig
{
    // public static class DependencyInjectionExtensions
    // {
    //     public static void AddStandardServices(this WebApplicationBuilder builder)
    //     {
    //         var securityScheme = new OpenApiSecurityScheme()
    //         {
    //             Name = "Authorization",
    //             Description = "JWT Authorization header info using bearer tokens",
    //             In = ParameterLocation.Header,
    //             Type = SecuritySchemeType.Http,
    //             Scheme = "bearer",
    //             BearerFormat = "JWT"
    //         };

    //         var securityRequirement = new OpenApiSecurityRequirement
    //         {
    //             {
    //                 new OpenApiSecurityScheme
    //                 {
    //                     Reference = new OpenApiReference
    //                     {
    //                         Type = ReferenceType.SecurityScheme,
    //                         Id = "bearerAuth",
    //                     }
    //                 },
    //                 new string[] { }
    //             }
    //         };

    //         builder.Services.AddSwaggerGen(options =>
    //         {
    //             options.AddSecurityDefinition("bearerAuth", securityScheme);
    //             options.AddSecurityRequirement(securityRequirement);
    //             options.OperationFilter<FileUploadOperationFilter>();
    //         });
    //     }

    //     public static void AddApiVersioningServices(this WebApplicationBuilder builder) { }

    //     public static void AddAuthServices(this WebApplicationBuilder builder)
    //     {
    //         builder.Services.AddCors(options =>
    //         {
    //             options.AddPolicy(
    //                 "AllowSpecificOrigin",
    //                 builder =>
    //                 {
    //                     builder
    //                         .WithOrigins("http://localhost:5094")
    //                         .AllowAnyMethod()
    //                         .AllowAnyHeader()
    //                         .AllowCredentials();
    //                 }
    //             );
    //         });
    //         builder.Services.AddAuthorization(options =>
    //         {
    //             options.FallbackPolicy = new AuthorizationPolicyBuilder()
    //                 .RequireAuthenticatedUser()
    //                 .Build();
    //         });

    //         builder
    //             .Services.AddAuthentication("Bearer")
    //             .AddJwtBearer(opts =>
    //             {
    //                 opts.TokenValidationParameters = new()
    //                 {
    //                     ValidateIssuer = true,
    //                     ValidateAudience = true,
    //                     ValidateLifetime = true,

    //                     ValidateIssuerSigningKey = true,
    //                     ValidIssuer = builder.Configuration.GetValue<string>(
    //                         "Authentication:Issuer"
    //                     ),
    //                     ValidAudience = builder.Configuration.GetValue<string>(
    //                         "Authentication:Audience"
    //                     ),
    //                     IssuerSigningKey = new SymmetricSecurityKey(
    //                         Encoding.ASCII.GetBytes(
    //                             builder.Configuration.GetValue<string>("Authentication:SecretKey")!
    //                         )
    //                     )
    //                 };
    //             });
    //     }

    //     public static void AddDataAccessServices(this WebApplicationBuilder builder)
    //     {
    //         builder.Services.AddDbContext<AppDbContext>(options =>
    //             options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
    //         );

    //         builder
    //             .Services.AddIdentity<IdentityUser, IdentityRole>()
    //             .AddEntityFrameworkStores<AppDbContext>()
    //             .AddDefaultTokenProviders();

    //         builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
    //         builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    //         builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
    //         builder.Services.AddScoped<IProductRepository, ProductRepository>();
    //         builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
    //         builder.Services.AddScoped<IAuthService, AuthService>();
    //     }
    // }
}
