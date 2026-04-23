using LaptopServer.DB;
using LaptopServer.Infrastructure.API;
using LaptopServer.Infrastructure.API.NovaPost;
using LaptopServer.Service;
using LaptopServer.Services.Background_services;
using LaptopServer.Services.Background;
using LaptopServer.Infrastructure.Notification;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ILaptopService, LaptopService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<INovaPostDbService, NovaPostDbService>();
//Background services
builder.Services.AddHostedService<CartCleanerBgService>();
builder.Services.AddHostedService<NpCacheBgService>();
builder.Services.AddHostedService<NpInternetDocBgService>();

//HTTP Clients
builder.Services.AddHttpClient<INovaPostApiService, NovaPostApiService>().AddStandardResilienceHandler();
builder.Services.AddHttpClient<IMonopayService, MonopayService>((provider, client) =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var key = config["ApiKeys:Monopay"] ?? throw new InvalidOperationException("Null Monopay token");
    client.DefaultRequestHeaders.Add("X-Token", key);
}).AddStandardResilienceHandler();
//Other services
builder.Services.AddMemoryCache();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
//Channels
builder.Services.AddSingleton<OrderProcessingChannel>();
//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularOrigin",
        policy =>
        {
            policy.WithOrigins("https://localhost:50292", "https://potuzhni-laptops-atbmfyb4hafdhyb4.polandcentral-01.azurewebsites.net")
            .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
        );
});
//DBContext
builder.Services.AddDbContext<LaptopsDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//Identity and roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<LaptopsDBContext>()
.AddDefaultTokenProviders();
//JWT
var jwtSetting = builder.Configuration.GetSection("JwtSetting");
var key = Encoding.UTF8.GetBytes(jwtSetting["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSetting["LaptopServer"],
        ValidAudience = jwtSetting["LaptopClient"],
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();
// Configure the HTTP request pipeline.
app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options
        .AddPreferredSecuritySchemes("https")
        .WithTitle("LaptopServer")
        .WithTheme(ScalarTheme.Moon)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAngularOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LaptopsDBContext>();

    await dbContext.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "Admin", "User" };

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
            await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}
app.Run();