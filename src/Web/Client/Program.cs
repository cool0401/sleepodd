using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Podcast.Components;
using Podcast.Pages.Data;
using Podcast.Shared;
using Podcast.Shared.Interface;
using Microsoft.AspNetCore.Components.Authorization;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Podcast.Common;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["PodcastApi:BaseAddress"]!),
    DefaultRequestHeaders =
    {
        Accept = { new MediaTypeWithQualityHeaderValue("application/json") }
    }
}.EnableIntercept(sp));

builder.Services.AddScoped<AuthenticationStateProvider, PodcastAuthStateProvider>();

builder.Services.AddAuthorizationCore(config =>
{
	config.AddPolicy(Policies.IsAdmin, Policies.IsAdminPolicy());
	config.AddPolicy(Policies.IsUser, Policies.IsUserPolicy());
});
builder.Services.AddApiAuthorization();
builder.Services.AddHttpClientInterceptor();

builder.Services.AddScoped<ThemeInterop>();
builder.Services.AddScoped<IAudioInterop, AudioInterop>();
builder.Services.AddScoped<LocalStorageInterop>();
builder.Services.AddScoped<ClipboardInterop>();
builder.Services.AddScoped<HttpInterceptorService>();
builder.Services.AddScoped<SubscriptionsService>();
builder.Services.AddScoped<ListenLaterService>();
builder.Services.AddSingleton<PlayerService>();
builder.Services.AddScoped<ListenTogetherHubClient>(_ =>
    new ListenTogetherHubClient(builder.Configuration["ListenTogetherHub"]!));

builder.Services.AddScoped<IPodcastService, PodcastService>();
builder.Services.AddScoped<RefreshTokenService>();

await builder.Build().RunAsync();