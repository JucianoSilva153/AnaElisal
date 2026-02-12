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

builder.Services.AddHttpClient("ElisalApi", client => client.BaseAddress = new Uri("http://localhost:5090"))
    .AddHttpMessageHandler<JwtInterceptor>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ElisalApi"));

await builder.Build().RunAsync();
