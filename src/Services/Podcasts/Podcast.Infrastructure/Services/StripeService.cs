using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Podcast.Infrastructure.Services.Interfaces;
using Stripe;
using Stripe.Checkout;
using System.Net.NetworkInformation;

namespace Podcast.Infrastructure.Services
{
    public class StripeService : IStripeService
    {
		private readonly string _baseUrl;

		private readonly string _standardPriceId;

		private readonly string _premiumPriceId;


		public StripeService(IConfiguration configuration)
		{
			var applicationUrlConfig = configuration.GetSection("AppSettings:ApplicationUrl") ??
				 throw new NullReferenceException("'AppSettings:ApplicationUrl' not found.");
#if DEBUG || LOCALDEV
			_baseUrl = applicationUrlConfig["development"] ??
				throw new NullReferenceException("Application Url 'AppSettings:ApplicationUrl:development' not found.");
#else
            _baseUrl = applicationUrlConfig["production"] ??
                throw new NullReferenceException("Application Url 'AppSettings:ApplicationUrl:production' not found.");
#endif

			_standardPriceId = configuration.GetValue<string>("Stripe:StandardPriceId") ??
				 throw new NullReferenceException("'Stripe:StandardPriceId' not found.");

			_premiumPriceId = configuration.GetValue<string>("Stripe:PremiumPriceId") ??
				 throw new NullReferenceException("'Stripe:PremiumPriceId' not found.");
		}


		public async Task<Customer> CreateCustomerAsync(string email)
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
            };

            var service = new CustomerService();
            return await service.CreateAsync(options);
        }

        public async Task AttachPaymentMethodToCustomer(string customerId, string paymentMethodId)
        {
            var options = new PaymentMethodAttachOptions
            {
                Customer = customerId,
            };

            var service = new PaymentMethodService();
            await service.AttachAsync(paymentMethodId, options);

            // Optionally, set this payment method as the default for invoices
            var customerOptions = new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = paymentMethodId,
                },
            };

            var customerService = new CustomerService();
            await customerService.UpdateAsync(customerId, customerOptions);
        }

        public async Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId)
        {
            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId,
                    }
                },
				PaymentBehavior = "default_incomplete"
			};
			options.AddExpand("latest_invoice.payment_intent");

			var service = new SubscriptionService();
            return await service.CreateAsync(options);
        }


		public async Task<Session> CreateSessionAsync(string customerId, string subscription)
		{
			var options = new SessionCreateOptions
			{
				Customer = customerId,
				PaymentMethodTypes = new List<string>
			{
				"card",
			},
				LineItems = new List<SessionLineItemOptions>
			{
				new SessionLineItemOptions
				{
					Price = subscription == "Standard" ? _standardPriceId : _premiumPriceId,
					Quantity = 1
				},
			},
				Mode = "subscription",
				SuccessUrl = $"{_baseUrl}/subscribe/success?session_id={{CHECKOUT_SESSION_ID}}",
				CancelUrl = $"{_baseUrl}/checkout",
			};

			return await new SessionService().CreateAsync(options);
		}

		public Task<Subscription> CancelSubscriptionAsync(string customerId)
		{
			var options = new SubscriptionListOptions
			{
				Customer = customerId,
				Status = "active",
			};

			var service = new SubscriptionService();
			options.AddExpand("data.default_payment_method");
			var subscriptions = service.List(options);
			var subscription = service.Cancel(subscriptions.FirstOrDefault().Id, null);
			return Task.FromResult(subscription);
		}

		public Task<string> GetCurrentSubscriptionAsync(string customerId)
		{
			var options = new SubscriptionListOptions
			{
				Customer = customerId,
				Status = "active",
			};
			options.AddExpand("data.default_payment_method");
			var service = new SubscriptionService();
			var subscriptions = service.List(options);

			foreach (var subscription in subscriptions)
			{
				foreach (var item in subscription.Items)
				{
					if (item.Plan.Id == _standardPriceId)
					{
						return Task.FromResult("Standard");
					}
					if (item.Plan.Id == _premiumPriceId)
					{
						return Task.FromResult("Premium");
					}
				}
			}

			return Task.FromResult(string.Empty);
		}

        public Task<IEnumerable<Subscription>> GetSubscriptionsAsync(string customerId)
        {
            var options = new SubscriptionListOptions
            {
                Customer = customerId,
                Status = "all",
            };
            options.AddExpand("data.default_payment_method");
            var service = new SubscriptionService();
            return Task.FromResult<IEnumerable<Subscription>>(service.List(options));
        }
    }
}
