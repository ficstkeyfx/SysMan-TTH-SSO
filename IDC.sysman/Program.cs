global using SysMan.Data;
using IDC.Frontend.Services;
using IDC.Frontend.Services.ApiService;
using IDC.Frontend.Services.AuthService;
using IDC.Frontend.Services.AuthServices;
using IDC.Frontend.Services.HttpService;
using IDC.Frontend.Services.NumService;
using IDC.Frontend.Services.TokenService;
using IDC.Frontend.Services.TvfService;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Minio;
using Minio.DataModel.Args;
using SysMan;
using SysMan.Hubs;
using SysMan.Models;
using SysMan.Services.LDapServices;
using SysMan.Services.LinkService;
using Radzen;
using System.Net.Http.Headers;
using SysMan.Services.IApiKCServices;

var builder = WebApplication.CreateBuilder(args);
IConfigurationRoot configuration = new ConfigurationBuilder()
                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                            .AddJsonFile("appsettings.json")
                            .Build();
builder.Services.AddHttpClient("GenericApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.SetHandlerLifetime(TimeSpan.FromMinutes(5)); // refresh DNS định kỳ


// Add services to the container.
builder.Services.AddAuthenticationCore();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<ApiServices>();
builder.Services.AddScoped<LinkService>();
builder.Services.AddScoped<LdapUserService>();
builder.Services.AddScoped<DialogService>();
builder.Services.AddSingleton<ExchangeProvisioner>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITokenService,TokenService>();
builder.Services.AddScoped<IApiKCServices,ApiKCServices>();
builder.Services.AddScoped<AuthenServices>();
builder.Services.AddScoped<IAuthenServices,AuthenServices>();
builder.Services.AddScoped<IApiServices, ApiServices>();
builder.Services.AddScoped<ILinkService, LinkService>();
builder.Services.AddScoped<ITvfServices, TvfServices>();
builder.Services.AddScoped<TvfServices>();
builder.Services.AddScoped<INumServices, NumServices>();
builder.Services.AddScoped<NumServices>();
builder.Services.AddScoped<IHttpServices, HttpServices>();
builder.Services.AddScoped<HttpServices>();
builder.Services.AddScoped<IClientServices, ClientServices>();
builder.Services.AddScoped<ClientServices>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();


builder.Services.AddRadzenComponents();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
          new[] { "application/octet-stream" });
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = null; 
});

builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = null; 
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCollabora", policy =>
    {
        policy.WithOrigins("http://203.128.246.222:9980") 
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.Configure<CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});



var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Features.Get<IHttpMaxRequestBodySizeFeature>()!.MaxRequestBodySize = 1000 * 1024 * 1024; 
    await next.Invoke();
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Content-Security-Policy", 
        "frame-ancestors 'self' http://SysMan.6pg.org http://203.128.246.222:9980;");
    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseResponseCompression();
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowCollabora");
app.UseRouting();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapHub<ChatHubs>("/chat");
app.Run();
