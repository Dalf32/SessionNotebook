using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SessionNotebook;
using SessionNotebook.Data;
using SessionNotebook.Providers;
using SessionNotebook.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!) });

builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    options.ProviderOptions.DefaultScopes.Add("email");
    builder.Configuration.Bind("Oidc", options.ProviderOptions);
}).AddAccountClaimsPrincipalFactory<ApiAuthAccountFactory>();

ConfigureCommonServices(builder.Services);

await builder.Build().RunAsync();
return;

void ConfigureCommonServices(IServiceCollection services)
{
    services.AddScoped<NotebookState>();
    services.AddScoped<HistoryStack>();
    services.AddScoped<CampaignService>();
    services.AddScoped<SessionService>();
    services.AddScoped<NoteService>();
    services.AddScoped<NounService>();
    services.AddScoped<TagService>();
}
