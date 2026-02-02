
using System.Text;
using DT_I_Onboarding_Portal.Data;
using DT_I_Onboarding_Portal.Server.Middleware;
using DT_I_Onboarding_Portal.Core.Models;
using DT_I_Onboarding_Portal.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DT_I_Onboarding_Portal.Data.Stores;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal64954", policy =>
    {
        policy
            .WithOrigins("http://localhost:64954")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// 🔹 UPDATED: Add Swagger with Bearer auth definition
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DT-I Onboarding Portal API",
        Version = "v1"
    });

    //  Define the Bearer scheme (HTTP, not ApiKey)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization using the Bearer scheme. Example: **Bearer {your token}**",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,      
        Scheme = "bearer",                   
        BearerFormat = "JWT"
    });

    // Require Bearer auth by default for all operations
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("SmtpSettings"));
//builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();


var jwtSection = builder.Configuration.GetSection("Jwt");
var keyString = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(keyString))
{
    throw new InvalidOperationException("Jwt:Key is missing in configuration.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true; // keep true in production
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // avoid 5-min default skew issues
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = signingKey,

            // Ensure claims map consistently with your TokenService
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EfUserStore>();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowLocal64954");
app.UseAuthentication();

app.UseMiddleware<RoleMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var env = services.GetRequiredService<IHostEnvironment>();
    if (env.IsDevelopment())
    {
        // Resolve via DI rather than 'new'
        var userStore = services.GetRequiredService<EfUserStore>();
        // Constructor will handle seeding if empty
    }
}

app.Run();
