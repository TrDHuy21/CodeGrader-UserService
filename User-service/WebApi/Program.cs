
using Application.Services.Implement;
using Application.Services.Interface;
using Infrastructure.Context;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApi.Midleware;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<USContext>(option =>
            {
                option.UseSqlServer(builder.Configuration.GetConnectionString("UserConnection"));
            });

            // add unitofwork
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // add services
            builder.Services.AddTransient<IUserService,UserService>();
            builder.Services.AddTransient<IAuthService, AuthService>();
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddTransient<IAdminService, AdminService>();
            builder.Services.AddTransient<ICloudStorageService, CloudStorageService>();

            // add mapper
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMemoryCache();
            builder.Services.AddHttpContextAccessor();

            // get jwt
            var jwt = builder.Configuration.GetSection("Jwt");

            // get key
            var key = Encoding.UTF8.GetBytes(jwt["key"] ?? throw new InvalidOperationException("Key is missing"));

            // configuration jwt
            // dang ky dich vu xac thuc jwt
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    option.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwt["Issuer"],
                        ValidAudience = jwt["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1",
                    Description = "API có Authentication v?i JWT"
                });

                // add Security Definition cho Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Input JWT token  format: Bearer {token}"
                });

                // add Security Requirement
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseMiddleware<TokenValidationMidleware>();
            app.UseAuthorization();

            app.MapControllers();

            // Auto migration khi start app
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<USContext>();
                try
                {
                    // Tự động tạo database và chạy migration
                    context.Database.Migrate();

                }
                catch (Exception ex)
                {
                    // Log error nhưng không crash app
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }
            app.Run();
        }
    }
}
