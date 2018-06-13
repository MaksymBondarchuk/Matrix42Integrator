using System.Collections.Generic;
using Matrix42Integrator.SecurityLayer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Matrix42Integrator.Controllers
{
	[Route("[controller]/[action]")]
	public class AccountController : Controller
	{
		public AccountController(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
		{
			Configuration = configuration;
			HostingEnvironment = hostingEnvironment;
		}

		private IConfiguration Configuration { get; }
		private IHostingEnvironment HostingEnvironment { get; }

		[HttpGet]
		public IActionResult SignIn()
		{
			return RedirectToAction("Matrix42SignIn");
		}

		[HttpGet]
		public IActionResult SignOut()
		{
			HttpContext.SignOutAsync();
			return Redirect("/");
		}

		public IActionResult Matrix42SignIn()
		{
			var serverUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/account/gettoken";
			var endpoint = $"{Configuration["Matrix42:AuthorizationEndpoint"]}?client_id={Configuration["Matrix42:ClientId"]}&scope={Configuration["Matrix42:Scope"]}&redirect_uri={HttpUtility.UrlEncode(serverUrl)}&response_type=token";
			return Redirect(endpoint);
		}

		// ReSharper disable once InconsistentNaming
		public async Task<IActionResult> GetToken(string access_token)
		{
			ClaimsPrincipal principal = ValidateToken(access_token);

			var authenticationProperties = new AuthenticationProperties();
			authenticationProperties.StoreTokens(new List<AuthenticationToken>
			{
				new AuthenticationToken
				{
					Name = OpenIdConnectParameterNames.AccessToken,
					Value = access_token
				}
			});

			await HttpContext.SignInAsync(principal, authenticationProperties);

			return Redirect("/");
		}

		private ClaimsPrincipal ValidateToken(string token)
		{
			var handler = new JwtSecurityTokenHandler();

			return handler.ValidateToken(token, JwtHandler.GetTokenValidationParameters(), out _);
		}
	}
}