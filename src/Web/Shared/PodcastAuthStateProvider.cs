using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Podcast.Components;

namespace Podcast.Shared
{
    public class PodcastAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;
        private readonly LocalStorageInterop _localStorage;
        private readonly NavigationManager _navigationManager;

        public PodcastAuthStateProvider(HttpClient http, LocalStorageInterop localStorage, NavigationManager navigationManager)
        {
            _http = http;
            _localStorage = localStorage;
            _navigationManager = navigationManager;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await _localStorage.GetItem<string>("accessToken");


            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);

            var state = new AuthenticationState(
                    new ClaimsPrincipal(
                        new ClaimsIdentity(ParseClaimsFromJwt(savedToken), "jwt")));
            NotifyAuthenticationStateChanged(Task.FromResult(state));
            return state;
        }

        /// <summary>
        ///  Used to when a user logs in.
        /// </summary>
        /// <param name="email"></param>
        public async Task LoggedIn(string token)
        {
            var savedToken = await _localStorage.GetItem<string>("accessToken");
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void LoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
