using System.Net.Http.Headers;
using Toolbelt.Blazor;
using System.Net;
using Podcast.Shared;
using Microsoft.AspNetCore.Components;

namespace Podcast.Components
{
	public class HttpInterceptorService : IDisposable
	{
		private readonly HttpClientInterceptor _interceptor;
		private readonly RefreshTokenService _refreshTokenService;
		private readonly NavigationManager _navigationManager;

		public HttpInterceptorService(
			HttpClientInterceptor interceptor,
			RefreshTokenService refreshTokenService,
			NavigationManager navigationManager)
		{
			_interceptor = interceptor;
			_refreshTokenService = refreshTokenService;
			_navigationManager = navigationManager;

			RegisterEvent();
		}

		public void RegisterEvent()
		{
			//_interceptor.BeforeSendAsync += InterceptBeforeHttpAsync;
			_interceptor.AfterSend += InterceptAfterResponse;
		}
		public async Task InterceptBeforeHttpAsync(object sender, HttpClientInterceptorEventArgs e)
		{
			//var absPath = e.Request.RequestUri.AbsolutePath;
			//Console.WriteLine($"absPath : {absPath}");
			//if (!absPath.Contains("accounts"))
			var token = await _refreshTokenService.TryRefreshToken();
			if (!string.IsNullOrEmpty(token))
			{
				e.Request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			}
		}

		private void InterceptAfterResponse(object? sender, HttpClientInterceptorEventArgs e)
		{
            if (!e.Response.IsSuccessStatusCode)
			{
				var statusCode = e.Response.StatusCode;
				switch (statusCode)
				{
					//case HttpStatusCode.NotFound:
					//	_navigationManager.NavigateTo("/404", forceLoad: true);
					//	break;
					case HttpStatusCode.Unauthorized:
						_navigationManager.NavigateTo("/signin", forceLoad: true);
                        break;
                    //case HttpStatusCode.InternalServerError:
                    //    _navigationManager.NavigateTo("/500", forceLoad: true);
					//	break;
				}
			}
		}

		public void Dispose()
		{
			DisposeEvent();
		}

		public void DisposeEvent()
		{
			//_interceptor.BeforeSendAsync -= InterceptBeforeHttpAsync;
			_interceptor.AfterSend -= InterceptAfterResponse;
		}
	}
}
