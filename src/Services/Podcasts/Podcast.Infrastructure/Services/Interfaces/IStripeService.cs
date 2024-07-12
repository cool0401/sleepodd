using Stripe;
using Stripe.Checkout;

namespace Podcast.Infrastructure.Services.Interfaces
{
    public interface IStripeService
    {
        public Task<Customer> CreateCustomerAsync(string email);

        public Task AttachPaymentMethodToCustomer(string customerId, string paymentMethodId);

        public Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId);

        public Task<Session> CreateSessionAsync(string customerId, string subscription);

        public Task<Subscription> CancelSubscriptionAsync(string customerId);

		public Task<string> GetCurrentSubscriptionAsync(string customerId);

        public Task<IEnumerable<Subscription>> GetSubscriptionsAsync(string customerId);

    }
}
