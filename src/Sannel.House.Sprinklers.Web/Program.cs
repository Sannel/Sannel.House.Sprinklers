using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Sannel.House.Sprinklers.Web;
using Sannel.House.Sprinklers.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Auth (Azure AD)
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(
        builder.Configuration["ApiScopes"] ?? "api://sprinklers/user_impersonation");
});

// Configure SprinklersClient options
builder.Services.Configure<SprinklerClientOptions>(options =>
{
    options.HostUri = new Uri(builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress);
});

// API HTTP client using SprinklersClient
builder.Services.AddHttpClient<SprinklersClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress);
});

builder.Services.AddMudServices();

await builder.Build().RunAsync();
