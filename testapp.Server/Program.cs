using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Data;
using System.Text;
using testapp.DAL.Context;
using testapp.DAL.Interfaces;
using testapp.DAL.Repositories;
using testapp.Domain.Interfaces;
using testapp.Domain.Services;
using testapp.Server.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("ReportConnection")));
builder.Services.AddSingleton<DapperConnectionFactory>();
builder.Services.AddHttpContextAccessor();

// Loggin
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration) // read appsettings.json
        .Enrich.FromLogContext();
    configuration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
    configuration.MinimumLevel.Override("System", LogEventLevel.Warning);
});

// JWT Authentication
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewReport", policy =>
        policy.RequireClaim("permission", "ViewReport"));
});


builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IRolePermissionRepo, RolePermissionRepo>();
builder.Services.AddScoped<ILoginLogRepo, LoginLogRepo>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMainReportService, MainReportService>();
builder.Services.AddScoped<IMainReportRepo, MainReportRepo>();
builder.Services.AddScoped<IAppLogRepo, AppLogRepo>();
builder.Services.AddScoped<IAppLogService, AppLogService>();

var app = builder.Build();

app.UseDeveloperExceptionPage();  // Shows detailed error messages

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapOpenApi();

app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyHeader()
          .AllowAnyMethod());

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
