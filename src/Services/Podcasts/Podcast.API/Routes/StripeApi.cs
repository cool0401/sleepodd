using Microsoft.AspNetCore;
using Stripe;

namespace Podcast.API.Routes
{
    public static class StripeApi
    {
        public static RouteGroupBuilder MapStripeApi(this RouteGroupBuilder group)
        {
            group.MapGet("/webhook", Webhook).WithName("Webhook");
            return group;
        }

        public static async ValueTask<Results<Ok<string>, BadRequest<string>>> Webhook(
            HttpContext httpContext,
            PodcastDbContext podcastDbContext,
            IConfiguration configuration,
            CancellationToken cancellationToken)
        {
            string json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync(cancellationToken);

            var _webhookSecret = configuration.GetValue<string>("Stripe:WebhookSecret") ??
            throw new NullReferenceException("'Stripe:WebhookSecret' not found.");

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    httpContext.Request.Headers["Stripe-Signature"], _webhookSecret);
                switch (stripeEvent.Type)
                {
                    case Events.CustomerSubscriptionCreated:
                        break;
                    case Events.CustomerSubscriptionDeleted:
                        break;
                    case Events.CustomerSubscriptionPendingUpdateApplied:
                        break;
                    case Events.CustomerSubscriptionPendingUpdateExpired:
                        break;
                    case Events.CustomerSubscriptionResumed:
                        break;
                    case Events.CustomerSubscriptionTrialWillEnd:
                        break;
                    case Events.CustomerSubscriptionUpdated:
                        break;
                    case Events.InvoiceCreated:
                        break;
                    case Events.InvoiceUpcoming:
                        break;
                    case Events.SubscriptionScheduleAborted:
                        break;
                    case Events.SubscriptionScheduleCanceled:
                        break;
                    case Events.SubscriptionScheduleCompleted:
                        break;
                    case Events.SubscriptionScheduleCreated:
                        break;
                    case Events.SubscriptionScheduleExpiring:
                        break;
                    case Events.SubscriptionScheduleReleased:
                        break;
                    case Events.SubscriptionScheduleUpdated:
                        break;

                }
                return TypedResults.Ok("");
            }
            catch (StripeException e)
            {
                return TypedResults.BadRequest(e.Message);
            }
        }
    }
}
