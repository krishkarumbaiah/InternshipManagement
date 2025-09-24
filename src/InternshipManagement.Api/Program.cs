using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InternshipManagement.Api.Data;
using InternshipManagement.Api.Models;
using InternshipManagement.Api.Services; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using InternshipManagement.Api.Config; 

var builder = WebApplication.CreateBuilder(args);

// Add configuration sources
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// DbContext
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(conn));

// Identity (int keys)
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders(); 

// JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = jwtSettings.GetValue<string>("Key");
var keyBytes = Encoding.UTF8.GetBytes(key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        ValidateAudience = true,
        ValidAudience = jwtSettings.GetValue<string>("Audience"),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddScoped<IAppEmailSender, EmailSender>();

//  Register Email service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Existing hosted service
builder.Services.AddHostedService<NotificationService>();

var app = builder.Build();

// Apply migrations and seed DB (roles, admin user, master tables)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

    db.Database.Migrate();

    async Task SeedIdentity()
    {
        string[] roles = new[] { "Admin", "Intern", "Trainer" };
        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole<int>(r));
        }

        var adminEmail = "admin@ims.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Admin"
            };

            var createRes = await userManager.CreateAsync(adminUser, "Admin@12345!");
            if (createRes.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                foreach (var err in createRes.Errors) Console.WriteLine(err.Description);
            }
        }
    }

    async Task SeedMasterTables()
    {
        if (!db.TypeCdmt.Any())
        {
            db.TypeCdmt.AddRange(new[]
            {
                new TypeCdmt { Code = "BATCH_TYPE", Description = "Batch Type", Value = "Internship" },
                new TypeCdmt { Code = "USER_TYPE", Description = "User Type", Value = "Student" }
            });
        }

        if (!db.RowStatus.Any())
        {
            db.RowStatus.AddRange(new[]
            {
                new RowStatus { Name = "Active", Description = "Active row" },
                new RowStatus { Name = "Inactive", Description = "Inactive row" },
                new RowStatus { Name = "Deleted", Description = "Deleted row" }
            });
        }

        await db.SaveChangesAsync();
    }

    //  Seed courses
    async Task SeedCourses()
    {
        if (!db.Courses.Any())
        {
            db.Courses.AddRange(new[]
            {
                new Course { Title = "C# Basics", Description = "Learn fundamentals of C#" },
                new Course { Title = "ASP.NET Core", Description = "Build APIs with .NET" },
                new Course { Title = "Angular Fundamentals", Description = "Frontend with Angular" }
            });
            await db.SaveChangesAsync();
        }
    }

    SeedIdentity().GetAwaiter().GetResult();
    SeedMasterTables().GetAwaiter().GetResult();
    SeedCourses().GetAwaiter().GetResult(); 
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.Run();

