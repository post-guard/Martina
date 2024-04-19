using System.Text;
using Martina.Entities;
using Martina.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("MongoDB");
if (connectionString is null)
{
    throw new InvalidOperationException("Failed to MongoDB connection string.");
}

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Need 'Authorization' header using JWT token.",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    options =>
    {
        JsonWebTokenOption? jsonWebTokenOption =  builder.Configuration.GetSection(JsonWebTokenOption.OptionName)
            .Get<JsonWebTokenOption>();

        if (jsonWebTokenOption is null)
        {
            throw new InvalidOperationException("Failed to get JWT options");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jsonWebTokenOption.Issuer,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jsonWebTokenOption.JsonWebTokenKey)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };
    });
builder.Services.AddDbContext<MartinaDbContext>(options =>
{
    options.UseMongoDB(connectionString, "Martina");
});
builder.Services.Configure<JsonWebTokenOption>(
    builder.Configuration.GetSection(JsonWebTokenOption.OptionName));
builder.Services.AddScoped<UserService>();

WebApplication application = builder.Build();

if (application.Environment.IsDevelopment())
{
    application.UseSwagger();
    application.UseSwaggerUI();
}

application.UseAuthentication();
application.UseAuthorization();

application.UseStaticFiles();

application.MapControllers();
application.MapFallbackToFile("/index.html");

await application.RunAsync();
