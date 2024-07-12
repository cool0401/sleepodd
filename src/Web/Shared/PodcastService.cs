using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Podcast.Shared.Interface;
using Podcast.Components;
using Podcast.Common.DTOs;

namespace Podcast.Shared;

public class PodcastService : IPodcastService
{
    private readonly HttpClient _httpClient;

    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly LocalStorageInterop _localStorage;
    private readonly NavigationManager _navigation;

    public PodcastService(HttpClient httpClient,
        AuthenticationStateProvider authenticationStateProvider,
        LocalStorageInterop localStorage,
        NavigationManager navigation)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _localStorage = localStorage;
        _navigation = navigation;
    }

    public async Task<(SignInResultDto? signInResult, string? error, bool conflicted)> UserSignIn(string userEmail, string userPassword)
    {
        var signInDto = new SignInDto
        (
            userEmail,
            userPassword
        );

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("account/signin", signInDto);

        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<SignInResultDto>();
            
            await _localStorage.SetItem("accessToken", user.AccessToken);
			await _localStorage.SetItem("refreshToken", user.RefreshToken);
			await ((PodcastAuthStateProvider)_authenticationStateProvider).LoggedIn(user.AccessToken);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", user.AccessToken);

            return (user, null, false);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return (null, error, true);
            }
            return (null, error, false);
        }
    }


    public async Task<string> RefreshToken()
    {
        var token = await _localStorage.GetItem<string>("accessToken");
		var refreshToken = await _localStorage.GetItem<string>("refreshToken");
		try
        {
            var result = await _httpClient.PostAsJsonAsync("account/refreshtoken", new RefreshTokenDTO { AccessToken = token, RefreshToken = refreshToken });
			var rescon = await result.Content.ReadAsStringAsync();
			var refreshResult = await result.Content.ReadFromJsonAsync<SignInResultDto>();

			if (result.IsSuccessStatusCode)
            {
                await _localStorage.SetItem("accessToken", refreshResult.AccessToken);
				await _localStorage.SetItem("refreshToken", refreshResult.RefreshToken);
				_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", refreshResult.AccessToken);
			}
            else
            {
                _navigation.NavigateTo("/logout");
            }

            return refreshResult.AccessToken;
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Erorr is => {e}");
        }
    }

    public async Task UserSignOut()
    {
        await _localStorage.RemoveItem("accessToken");
        ((PodcastAuthStateProvider)_authenticationStateProvider).LoggedOut();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<(bool result, string? error)> ConfirmEmail(string userId, string token)
    {
        try
        {
            var response = await _httpClient.GetAsync($"account/confirmemail?userId={userId}&token={token}");

            if (response.IsSuccessStatusCode)
            {
                _ = await response.Content.ReadAsStringAsync();
                return (true, null);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, error.Trim('"'));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<(bool result, string? error)> SendConfirmEmailLink(string email)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"account/send-confirmemail?email={email}");

        if (response.IsSuccessStatusCode)
        {
            _ = await response.Content.ReadAsStringAsync();

            return (true, null);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error.Trim('"'));
        }
    }

    public async Task<UserDto?> GetUser(Guid guid) => await _httpClient.GetFromJsonAsync<UserDto>("account/user/" + guid);

    public async Task<(string? result, string? error)> UserSignUp(string fullName, string userName, string userEmail, string userPassword)
    {
        var signUpDto = new SignUpDto()
        {
            FullName = fullName,
            UserName = userName,
			Email = userEmail,
			Password = userPassword
		};

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("account/signup", signUpDto);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return (result.Trim('"'), null);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return (null, error.Trim('"'));
        }
    }

    public async Task<(string? result, string? error)> UserChange(string userName, string fullName, string userEmail)
    {
        var changeAccountDto = new ChangeAccountDto()
        {
            UserName = userName,
            FullName = fullName,
            Email = userEmail
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("account/change", changeAccountDto);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return (result.Trim('"'), null);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return (null, error.Trim('"'));
        }
    }

    public async Task<(bool result, string? error)> ForgotEmail(string email)
    {
        var forgotPasswordDto = new ForgotPasswordDto()
        {
            Email = email
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("account/forgot-password", forgotPasswordDto);

        if (response.IsSuccessStatusCode)
        {
            _ = await response.Content.ReadAsStringAsync();
            return (true, null);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error.Trim('"'));
        }
    }

    public async Task<(bool result, string? error)> ResetPassword(string id, string token, string password)
    {
        var resetPasswordDto = new ResetPasswordDto()
        {
            Id = id,
            Token = token,
            Password = password
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("account/reset-password", resetPasswordDto);

        if (response.IsSuccessStatusCode)
        {
            _ = await response.Content.ReadAsStringAsync();
            return (true, null);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error.Trim('"'));
        }
    }

    public async Task<(bool result, string? message)> Checkout(string subscription)
	{
		HttpResponseMessage response = await _httpClient.GetAsync($"account/checkout?subscription={subscription}");

		var result = await response.Content.ReadAsStringAsync();

		if (response.IsSuccessStatusCode)
		{
			return (true, result.Trim('"'));
		}
		else
		{
			return (false, result);
		}
	}

    public Task<BillingHistoryDto[]?> GetBillingHistoy() =>
        _httpClient.GetFromJsonAsync<BillingHistoryDto[]>("account/billing-history");

    public Task<Category[]?> GetCategories() =>
        _httpClient.GetFromJsonAsync<Category[]>("categories");

    public Task<Show[]?> GetShows(int limit, string? term = null) =>
        _httpClient.GetFromJsonAsync<Show[]>($"shows?limit={limit}&term={term}");

    public Task<Show[]?> GetShows(int limit, string? term = null, Guid? categoryId = null) =>
        _httpClient.GetFromJsonAsync<Show[]>($"shows?limit={limit}&term={term}&categoryId={categoryId}");

    public Task<Show?> GetShow(Guid id) =>
        _httpClient.GetFromJsonAsync<Show>($"shows/{id}");

	public Task<string?> GetCurrentSubscription() =>
		_httpClient.GetFromJsonAsync<string>($"account/subscription");

	public Task<string?> CancelSubscription() =>
		_httpClient.GetFromJsonAsync<string>($"account/cancel-subscription");

    public async Task<(bool result, string? message)> CreateShow(string title, string description, string image, string category)
    {
        var createShowDto = new CreateShowDto
        (
            title,
            description,
            image,
            category
        );

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("shows/", createShowDto);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            return (true, result.Trim('"'));
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return (false, error.Trim('"'));
        }
    }
}
