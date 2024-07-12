using Podcast.Common.DTOs;
using Podcast.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Podcast.API.Routes;

public static class AccountApi
{
    public static RouteGroupBuilder MapAccountApi(this RouteGroupBuilder group)
    {
        group.MapPost("/signin", SignIn).WithName("SignIn");
        group.MapPost("/signup", SignUp).WithName("SignUp");
        group.MapPost("/refreshtoken", RefreshToken).WithName("RefreshToken");
        group.MapGet("/confirmemail", ConfirmEmail).WithName("ConfirmEmail");
        group.MapGet("/send-confirmemail", SendConfirmEmailLink).WithName("SendConfirmeMail");
        group.MapGet("/user/{id}", Profile).RequireAuthorization().WithName("Profile");
		group.MapGet("/checkout", CreateCheckoutSession).RequireAuthorization().WithName("CreateCheckoutSession");
        group.MapPost("/change", Change).RequireAuthorization().WithName("Change");
        group.MapPost("/reset-password", ResetPassword).WithName("ResetPassword");
        group.MapPost("/forgot-password", ForgotPassword).WithName("ForgotPassword");
        group.MapGet("/cancel-subscription", CancelSubscription).RequireAuthorization().WithName("CancelSubscription");
		group.MapGet("/subscription", GetCurrentSubscription).RequireAuthorization().WithName("GetCurrentSubscription");
        group.MapGet("/billing-history", GetSubscriptions).RequireAuthorization().WithName("GetSubscriptions");
        group.MapGet("/version", GetVersion).WithName("GetVersion");
        return group;
    }
    public static async ValueTask<Results<BadRequest<string>, Conflict<string>, UnauthorizedHttpResult, NotFound<string>, Ok<SignInResultDto>>> SignIn(
        PodcastDbContext podcastDbContext, ITokenService tokenService,
        UserManager<User> userManager, SignInDto signInDto, CancellationToken cancellationToken)
    {
        if (signInDto is null) return TypedResults.BadRequest("Incorrect input");

        if (string.IsNullOrEmpty(signInDto.Email) || string.IsNullOrEmpty(signInDto.Password))
        {
            return TypedResults.BadRequest("Incorrect input");
        }

        var user = await userManager.FindByEmailAsync(signInDto.Email);

        if (user is null) 
            return TypedResults.NotFound("User with this email address not found.");

        if (!user.EmailConfirmed)
            return TypedResults.Conflict("User Email not confirmed yet.");

        var result = await userManager.CheckPasswordAsync(user, signInDto.Password);
        if (!result) return TypedResults.BadRequest("Incorrect password.");

		var claims = await tokenService.GetClaims(user);
		var accessToken = tokenService.GenerateAccessToken(claims);
		var refreshToken = tokenService.GenerateRefreshToken();
		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

		await userManager.UpdateAsync(user);

		return TypedResults.Ok(new SignInResultDto()
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            AccessToken = accessToken,
			RefreshToken = refreshToken
		});
    }

    public static async ValueTask<Results<BadRequest<string>, BadRequest<IEnumerable<IdentityError>>, Conflict<string>, Ok<string>>> SignUp(
        PodcastDbContext podcastDbContext, 
        SignUpDto dto, 
        UserManager<User> userManager, 
        IEmailService emailService,
        IStripeService stripeService,
        CancellationToken cancellationToken)
    {
        if (dto is null) return TypedResults.BadRequest("Incorrect input");


        if (string.IsNullOrEmpty(dto.FullName) || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
        {
            return TypedResults.BadRequest("Incorrect input");
        }

        if (!ValidateHelper.IsValidEmail(dto.Email))
        {
            return TypedResults.BadRequest("Incorrect email");
        }

        if (userManager.Users.Any(u => u.Email == dto.Email))
            return TypedResults.Conflict($"Invalid {dto.Email}: A user with this email address already exists.");

        if (string.IsNullOrWhiteSpace(dto.UserName))
        {
            dto.UserName = string.Join('_', dto.FullName.Split(' ')).ToLower();
            if (userManager.Users.Any(u => u.UserName == dto.UserName))
                dto.UserName = dto.UserName + '_' + Guid.NewGuid().ToString("N").Substring(0, 4);
        }
        else if (userManager.Users.Any(u => u.UserName == dto.UserName))
            return TypedResults.Conflict($"Invalid {dto.UserName}: A user with this username already exists.");

        var newUser = new User(dto);

		var stripeResult = await stripeService.CreateCustomerAsync(dto.Email);

        newUser.CustomerId = stripeResult.Id;

		var result = await userManager.CreateAsync(newUser, dto.Password);

		// Add all new users to the User role
		await userManager.AddToRoleAsync(newUser, "User");

		if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors.FirstOrDefault()?.Description);
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
        var validEmailToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var confirmationUrl = $"/account/confirmemail?userid={newUser.Id}&token={validEmailToken}";

        var emailResult = await emailService.SendWelcomeEmailAsync(dto.Email, confirmationUrl);

        if (emailResult && stripeResult is not null)
        {
            return TypedResults.Ok("Signup successful");
        }
        else
        {
            return TypedResults.BadRequest("Sending email failed!");
        }
    }

    public static async ValueTask<Results<BadRequest<string>, NotFound<string>, Ok<string>>> Change(
        ChangeAccountDto dto,
        UserManager<User> userManager,
        IEmailService emailService
        )
    {
        if (dto is null) return TypedResults.BadRequest("Incorrect input");

        if (string.IsNullOrEmpty(dto.FullName) || string.IsNullOrEmpty(dto.Email))
        {
            return TypedResults.BadRequest("Incorrect input");
        }

        if (!ValidateHelper.IsValidEmail(dto.Email))
        {
            return TypedResults.BadRequest("Incorrect email");
        }

        var user = await userManager.FindByNameAsync(dto.UserName);
        if (user == null)
            return TypedResults.NotFound("User not found");
        
        user.Name = dto.FullName;

        bool emailChanged = !String.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase);

        user.Email = dto.Email;

        if (emailChanged) user.EmailConfirmed = false;

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return TypedResults.BadRequest(result.Errors.FirstOrDefault()?.Description);
        }

        if (emailChanged)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var validEmailToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var confirmationUrl = $"/account/confirmemail?userid={user.Id}&token={validEmailToken}";

            var emailResult = await emailService.SendWelcomeEmailAsync(dto.Email, confirmationUrl);

            if (emailResult)
            {
                return TypedResults.Ok("Change Profile successful");
            }
            else
            {
                return TypedResults.BadRequest("Sending email failed!");
            }
        }
        else
        {
            return TypedResults.Ok("Change Profile successful");
        }
    }

    public static async ValueTask<Results<BadRequest<string>, NotFound<string>, Ok<string>>> ResetPassword(
        ResetPasswordDto dto,
        UserManager<User> userManager,
        IEmailService emailService)
    {
        if (dto is null) return TypedResults.BadRequest("Incorrect input");
        
        var user = await userManager.FindByIdAsync(dto.Id);
        if (user == null)
            return TypedResults.NotFound("User not found");

        var decodedToken = WebEncoders.Base64UrlDecode(dto.Token);
        string normalToken = Encoding.UTF8.GetString(decodedToken);

        var resetPassResult = await userManager.ResetPasswordAsync(user, normalToken, dto.Password);
        if (!resetPassResult.Succeeded)
        {
            return TypedResults.BadRequest(resetPassResult.Errors.FirstOrDefault().Description);
        }
        else
        {
            return TypedResults.Ok("Change Password successful.");
        }
    }

    public static async ValueTask<Results<BadRequest<string>, NotFound<string>, Ok<string>>> ForgotPassword(
        ForgotPasswordDto dto,
        UserManager<User> userManager,
         IEmailService emailService)
    {
        if (dto is null) return TypedResults.BadRequest("Incorrect input");
        
        if (!ValidateHelper.IsValidEmail(dto.Email))
        {
            return TypedResults.BadRequest("Incorrect email");
        }

        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return TypedResults.NotFound("User not found");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        
        var validPasswordToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var confirmationUrl = $"/reset-password?userid={user.Id}&token={validPasswordToken}";

        var emailResult = await emailService.SendWelcomeEmailAsync(dto.Email, confirmationUrl);

        if (emailResult)
        {
            return TypedResults.Ok("Sending email successful!");
        }
        else
        {
            return TypedResults.BadRequest("Sending email failed!");
        }
    }

    public static async ValueTask<Results<BadRequest<string>, NotFound<string>, Ok<string>>> ConfirmEmail(
        string userId, string token, UserManager<User> userManager
    )
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return TypedResults.BadRequest("Incorrect request");


        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return TypedResults.NotFound("User not found");


        var decodedToken = WebEncoders.Base64UrlDecode(token);
        string normalToken = Encoding.UTF8.GetString(decodedToken);
        var result = await userManager.ConfirmEmailAsync(user, normalToken);

        if (result.Succeeded)
            return TypedResults.Ok("Email confirmed successfully");

        return TypedResults.BadRequest(result.Errors.FirstOrDefault()?.Description);
    }

    public static async ValueTask<Results<BadRequest<string>, NotFound<string>, Ok<string>>> SendConfirmEmailLink(
        UserManager<User> userManager, IEmailService emailService, string email
    )
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return TypedResults.NotFound("User with this email address not found.");

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var validEmailToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var confirmationUrl = $"/account/confirmemail?userid={user.Id}&token={validEmailToken}";

        var emailResult = await emailService.SendWelcomeEmailAsync(email, confirmationUrl);
        if (emailResult)
        {
            return TypedResults.Ok("Sending email successful");
        }
        else
        {
            return TypedResults.BadRequest("Sending email failed!");
        }
    }

    public static async ValueTask<Results<BadRequest<string>, Ok<SignInResultDto>>> RefreshToken
    (
		RefreshTokenDTO tokenDto,
		ITokenService tokenService,
		UserManager<User> userManager,
		PodcastDbContext podcastDbContext
    )
    {
		var principal = tokenService.GetPrincipalFromExpiredToken(tokenDto.AccessToken);
		var username = principal.Identity.Name;
		var user = userManager.Users.SingleOrDefault(u => u.UserName == username);

		if (user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
		{
			return TypedResults.BadRequest("Invalid client request");
		}
		var claims = await tokenService.GetClaims(user);
		var newAccessToken = tokenService.GenerateAccessToken(claims);
		var newRefreshToken = tokenService.GenerateRefreshToken();

		user.RefreshToken = newRefreshToken;
		user.RefreshTokenExpiryTime = DateTime.Now.AddDays(5);

		await userManager.UpdateAsync(user);

		return TypedResults.Ok(new SignInResultDto()
		{
			Id = user.Id,
			Email = user.Email,
			UserName = user.UserName,
			AccessToken = newAccessToken,
			RefreshToken = newRefreshToken
		});
    }

    public static async ValueTask<Results<NoContent, Ok<UserDto>>> Profile(PodcastDbContext podcastDbContext, Guid id)
    {
        var user = await podcastDbContext.Users.FindAsync(id);

        if (user is null) return TypedResults.NoContent();

        return TypedResults.Ok(new UserDto());
    }

	public static async ValueTask<Results<NotFound<string>, Ok<string>>> CreateCheckoutSession(
		HttpContext context,
		string subscription,
		UserManager<User> userManager,
		IStripeService stripeService)
	{
        var user = await userManager.FindByNameAsync(context.User.Identity.Name);

		if (user is null)
			return TypedResults.NotFound("User with this email address not found.");

		var session = await stripeService.CreateSessionAsync(user.CustomerId, subscription);

		return TypedResults.Ok(session.Id);
	}


	public static async ValueTask<Results<NotFound<string>, Ok<string>>> CancelSubscription(
		HttpContext context,
		UserManager<User> userManager,
		IStripeService stripeService)
	{
		var user = await userManager.FindByNameAsync(context.User.Identity.Name);

		if (user is null)
			return TypedResults.NotFound("User with this email address not found.");

		var result = await stripeService.CancelSubscriptionAsync(user.CustomerId);

		return TypedResults.Ok(result.Id);
	}

	public static async ValueTask<Results<NotFound<string>, Ok<string>>> GetCurrentSubscription(
		HttpContext context,
		UserManager<User> userManager,
		IStripeService stripeService
		)
	{
		var user = await userManager.FindByNameAsync(context.User.Identity.Name);

		if (user is null)
			return TypedResults.NotFound("User with this email address not found.");

        var currentPlan = await stripeService.GetCurrentSubscriptionAsync(user.CustomerId);
        
		return TypedResults.Ok(currentPlan);
	}

    public static async ValueTask<Results<NotFound<string>, Ok<List<BillingHistoryDto>>>> GetSubscriptions(
        HttpContext context,
        UserManager<User> userManager,
        IStripeService stripeService
        )
    {
        var user = await userManager.FindByNameAsync(context.User.Identity.Name);

        if (user is null)
            return TypedResults.NotFound("User with this email address not found.");

        var history = await stripeService.GetSubscriptionsAsync(user.CustomerId);

        List<BillingHistoryDto> result = new List<BillingHistoryDto>();

        foreach ( var subscription in history)
        {
            if (subscription is null) continue;
            
        }

        return TypedResults.Ok(result);
    }

    public static async ValueTask<Ok<string>> GetVersion(HttpContext context)
    {
        return TypedResults.Ok("v2.0");
    }
}

