using CustomIdentity.Core.HelperModels;
using CustomIdentity.Core.Services.Authentication;
using CustomIdentity.Core.TokenGenerator;
using CustomIdentity.Domain.DatabaseModels.Identities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using ActionStatus = CustomIdentity.Core.HelperModels.ActionStatus;


namespace CustomIdentity.Core.Middlewares.Jwt
{

	public class JwtMiddleware : IMiddleware
	{
		private readonly IAuthService _authenticationService;
		private readonly ITokenGenerator _tokenGenerator;

		public JwtMiddleware(IAuthService authenticationService, ITokenGenerator tokenGenerator)
		{
			_authenticationService = authenticationService;
			_tokenGenerator = tokenGenerator;
		}


		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			try
			{
				if (!context.Request.Headers.ContainsKey("Authorization"))
				{
					await next(context);
					return;
				}


				string authorizationToken = context.Request.Headers["Authorization"].ToString().Split(' ')[1];
				bool validateToken = await _tokenGenerator.ValidateTokenAsync(authorizationToken);
				if (validateToken != true)
				{
					context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
					return;
				}

				bool validateAuthTokenTime = await _tokenGenerator.ValidateTokenLifeTimeAsync(authorizationToken);
				if (validateAuthTokenTime == true)
				{
					await next(context);
					return;
				}

				string? refreshToken = context.Request.Cookies["refreshToken"];
				if (refreshToken is null)
				{
					context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
					await _authenticationService.LogoutAsync();
					return;

				}

				System.Boolean validateRefreshToken = await _tokenGenerator.ValidateFullTokenAsync(refreshToken);
				if (validateRefreshToken != true)
				{
					context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
					await _authenticationService.LogoutAsync();
					return;
				}

				int userId = _tokenGenerator.GetUserIdFromAuthorizationToken(authorizationToken);
				ActionResultM<User> getUser = await _authenticationService.GetUserAsync(userId);
				if (getUser.Status is not ActionStatus.Success)
				{
					context.Response.StatusCode = (int)HttpStatusCode.NotFound;
					return;
				}

				User user = getUser.Data;
				var assignNewTokens = await _authenticationService.GiveTokensAsync(user);
				if (assignNewTokens.Status is not ActionStatus.Success)
				{
					context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
					return;
				}

				await next(context);
			}
			catch (Exception e)
			{
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				await context.Response.WriteAsync("Something is wrong with JWT middleware :( ");
				return;
			}
		}


	}
}
