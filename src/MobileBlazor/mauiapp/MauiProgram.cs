using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Infrastructure;
using Microsoft.AspNetCore.Components.Authorization;
using NetPodsMauiBlazor.Services;
using Podcast.Components;
using Podcast.Pages.Data;
using Podcast.Shared;
using Podcast.Common;
using Podcast.Shared.Interface;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace NetPodsMauiBlazor;

public static class MauiProgram
{
    /*public static string BaseWeb = $"{Base}:5002/listentogether";
    public static string Base = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2" : "http://localhost";
    public static string APIUrl = $"{Base}:5003/";
    public static string ListenTogetherUrl = $"{Base}:5001/listentogether";*/

    public static string BaseWeb = $"https://aisleepod-webapp.azurewebsites.net/";
    public static string APIUrl = $"https://aisleepodapica.thankfulhill-e80f92b5.eastus.azurecontainerapps.io";
    public static string ListenTogetherUrl = $"https://aisleepod-hub.azurewebsites.net/listentogether";

    //public static string BaseWeb = $"http://192.168.2.116:5002";
    //public static string APIUrl = $"http://192.168.2.116:5003";
    //public static string ListenTogetherUrl = $"http://192.168.2.116:5001/listentogether";

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(APIUrl),
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

#if WINDOWS
        builder.Services.AddSingleton<SharedMauiLib.INativeAudioService, SharedMauiLib.Platforms.Windows.NativeAudioService>();
#elif ANDROID
        builder.Services.AddSingleton<SharedMauiLib.INativeAudioService, SharedMauiLib.Platforms.Android.NativeAudioService>();
#elif MACCATALYST
        builder.Services.AddSingleton<SharedMauiLib.INativeAudioService, SharedMauiLib.Platforms.MacCatalyst.NativeAudioService>();
#elif IOS
        builder.Services.AddSingleton<SharedMauiLib.INativeAudioService, SharedMauiLib.Platforms.iOS.NativeAudioService>();
#endif

        builder.Services.AddScoped<ThemeInterop>();
        builder.Services.AddScoped<IAudioInterop, AudioInteropService>();
        builder.Services.AddScoped<LocalStorageInterop>();
        builder.Services.AddScoped<ClipboardInterop>();
        builder.Services.AddScoped<HttpInterceptorService>();
        builder.Services.AddScoped<SubscriptionsService>();
        builder.Services.AddScoped<ListenLaterService>();
        builder.Services.AddSingleton<PlayerService>();
        builder.Services.AddScoped<ListenTogetherHubClient>(_ =>
            new ListenTogetherHubClient(ListenTogetherUrl));
        builder.Services.AddScoped<ComponentStatePersistenceManager>();
        builder.Services.AddScoped<PersistentComponentState>(sp => sp.GetRequiredService<ComponentStatePersistenceManager>().State);

        builder.Services.AddScoped<IPodcastService, PodcastService>();
        builder.Services.AddScoped<RefreshTokenService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        return builder.Build();
    }
}
