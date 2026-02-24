using Microsoft.AspNetCore.Components.Authorization;
using Elisal.WasteManagement.UI.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Elisal.WasteManagement.UI;
using Microsoft.AspNetCore.Components.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<ApiAuthenticationStateProvider>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<ApiAuthenticationStateProvider>());
builder.Services.AddTransient<JwtInterceptor>();

using var http = new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
};

var config = new ConfigurationBuilder()
    .AddJsonStream(await http.GetStreamAsync("appsettings.json"))
    .AddJsonStream(await http.GetStreamAsync($"appsettings.{builder.HostEnvironment.Environment}.json"))
    .Build();

var apiBaseUrl = config["ApiSettings:BaseUrl"];

builder.Services.AddHttpClient("ElisalApi", client => client.BaseAddress = new Uri(apiBaseUrl ?? ""))
    .AddHttpMessageHandler<JwtInterceptor>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ElisalApi"));

await builder.Build().RunAsync();
