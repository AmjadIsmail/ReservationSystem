using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using ReservationSystem.Domain.DBContext;
using ReservationSystem.Domain.Models;
using ReservationSystem.Domain.Repositories;
using ReservationSystem.Domain.Service;
using ReservationSystem.Infrastructure.Repositories;
using ReservationSystem.Infrastructure.Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
builder.Services.AddScoped<ITravelBoardSearchRepository, TravelBoardSearchRepository>();
builder.Services.AddScoped<IFlightPriceRepository, FlightPriceRepository>();
builder.Services.AddScoped<IFlightOrderRepository, FlightOrderRepository>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<IFareCheckRepository, FareCheckRepository>();
builder.Services.AddScoped<IAirSellRepository, AirsellRepository>();
builder.Services.AddScoped<IAddPnrMultiRepository, AddPnrMultiRepository>();
builder.Services.AddScoped<IFopRepository, FopRepository>();
builder.Services.AddScoped<IPricePnrRepository, PricePnrRepository>();
builder.Services.AddScoped<ITicketTstRepository, TicketTstRepository>();
builder.Services.AddScoped<IHelperRepository, HelperRepository>();
builder.Services.AddScoped<IDBRepository, DBRepository>();
builder.Services.AddScoped<IPnrRetreiveRepository, PnrRetreiveRepository>();
builder.Services.AddScoped<IPnrQueueRepository, PnrQueueRepository>();
builder.Services.AddScoped<IDocIssueTicketRepository, DocIssueTicketRepository>();
builder.Services.AddScoped<IPnrCancelRepository, PnrCancelRepository>();
builder.Services.AddScoped<ITicketCancelRepository, TicketCancelRepository>();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression();

builder.Services.AddScoped<IEmailService,EmailService>(provider =>
{
    var configuration = builder.Configuration.GetSection("EmailSettings");
    var smtpServer = builder.Configuration["EmailSettings:SmtpServer"];
    var smtpPort = int.Parse(builder.Configuration["EmailSettings:SmtpPort"]);
    var smtpUser = builder.Configuration["EmailSettings:SmtpUser"];
    var smtpPass = "";//builder.Configuration["EmailSettings:SmtpPass"];
    return new EmailService(smtpServer, smtpPort, smtpUser, smtpPass);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin",
        builder => builder
            .WithOrigins("http://localhost:5173").WithOrigins("http://localhost:5273").WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();
builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

builder.Services.AddDbContext<DB_Context>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Configuration
   // .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
  //  .AddEnvironmentVariables();
var configuration = new ConfigurationBuilder()
   .SetBasePath(Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", false)
   .AddEnvironmentVariables()
   .Build();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapPost("/security/createToken",
[AllowAnonymous] (User user) =>
{
    if (user.UserName == builder.Configuration["Security:UserName"] && user.Password == builder.Configuration["Security:Password"])
    {
        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var key = Encoding.ASCII.GetBytes
        (builder.Configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials
            (new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha512Signature)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);
        var stringToken = tokenHandler.WriteToken(token);
        return Results.Ok(stringToken);
    }
    return Results.Unauthorized();
});
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseRouting();
app.UseCors("AllowOrigin");
app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var cacheService = services.GetRequiredService<ICacheService>();
        cacheService.LoadDataIntoCache();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while loading cache data.");
    }
}
app.Run();
