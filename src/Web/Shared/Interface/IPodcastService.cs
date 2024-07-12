using Podcast.Common.DTOs;

namespace Podcast.Shared.Interface
{
    public interface IPodcastService
    {

        public Task<(SignInResultDto? signInResult, string? error, bool conflicted)> UserSignIn(string userEmail, string userPassword);

        public Task<string> RefreshToken();

        public Task UserSignOut();

        public Task<(string? result, string? error)> UserSignUp(string fullName, string userName, string userEmail, string userPassword);

        public Task<(string? result, string? error)> UserChange(string userName, string fullName, string userEmail);

        public Task<(bool result, string? error)> ConfirmEmail(string userId, string token);

        public Task<(bool result, string? error)> ForgotEmail(string email);

        public Task<(bool result, string? error)> ResetPassword(string id, string token, string password);

        public Task<(bool result, string? error)> SendConfirmEmailLink(string email);

        public Task<(bool result, string? message)> Checkout(string subscription);

		public Task<string?> CancelSubscription();

		public Task<string?> GetCurrentSubscription();

		public Task<UserDto?> GetUser(Guid guid);

        public Task<Category[]?> GetCategories();

        public Task<Show[]?> GetShows(int limit, string? term = null);

        public Task<Show[]?> GetShows(int limit, string? term = null, Guid? categoryId = null);

        public Task<Show?> GetShow(Guid id);

        public Task<BillingHistoryDto[]?> GetBillingHistoy();

        public Task<(bool result, string? message)> CreateShow(string title, string description, string image,
            string category);

    }
}
