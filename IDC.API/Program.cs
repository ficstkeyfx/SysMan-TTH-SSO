using Microsoft.EntityFrameworkCore;
using IDC.Backend.Servives.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using api.Models;
var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddAutoMapper(typeof(GenericMappingProfile)); // Register AutoMapper
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var connStr = builder.Configuration.GetConnectionString("dbConnection")!;

// Register AddApiSerive with your DbContext
builder.Services.AddApiSerive<dbAPIContext>(connStr, dyn =>
{
    dyn.EnableClientExpressions = true; // set to false to disable dynamic filter/order strings
    // dyn.AllowedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase){ "IdUser", "UserName", "FullName" };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(120)));
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                    .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowCollabora", policy =>
//    {
//        policy.WithOrigins("http://203.128.246.222:9980") 
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowCollabora");
app.UseRouting();

app.UseHttpsRedirection();
app.UseOutputCache();
// ✨ THÊM DÒNG NÀY TRƯỚC UseAuthorization:
app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();

app.Run();

