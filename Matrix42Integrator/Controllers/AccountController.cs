using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Matrix42Integrator.Controllers
{
	[Route("[controller]/[action]")]
	public class AccountController : Controller
	{
		private HttpClient HttpClient { get; } = new HttpClient();
		private readonly IHostingEnvironment _hostingEnvironment;

		public AccountController(IHostingEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
		}


		[HttpGet]
		public IActionResult Login(string returnUrl = "/")
		{
			return RedirectToAction("Authorize");
			//return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl });
		}

		public async Task<IActionResult> Authorize()
		{
			return Redirect("https://accounts.matrix42.com/issue/oauth2/authorize?client_id=cf9ac74c-fefc-4d1c-9fcb-0c5cecd5203d&scope=urn%3a0580150c-7d26-422e-983c-53b211c44a3e&redirect_uri=http%3a%2f%2flocalhost%3a27923%2faccount%2fgettoken&response_type=token");
		}

		public async Task<IActionResult> GetToken(string access_token)
		{
			ClaimsPrincipal principal = ValidateToken(access_token);

			return SignIn(principal, JwtBearerDefaults.AuthenticationScheme);

			//if (token != null)
			//{
			//	var userIdentity = new ClaimsIdentity("Identity")
			//	{
			//		Label = "Identity"
			//	};
			//	userIdentity.AddClaims(token.Claims);
			//	//userIdentity.AddClaim(new Claim(ClaimTypes.Name, token.Claims));
			//	//userIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "1"));
			//	//userIdentity.AddClaim(new Claim(ClaimTypes.Country, "PL"));
			//	//userIdentity.AddClaim(new Claim(ClaimTypes.Email, "sampleuser@test.pl"));
			//	//userIdentity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
			//	//userIdentity.AddClaim(new Claim(ClaimTypes.Role, "User"));
			//	var identity = new ClaimsPrincipal(userIdentity);

			//	return SignIn(identity, JwtBearerDefaults.AuthenticationScheme);
			//}
			// https://accounts.matrix42.com/issue/oauth2/authorize?client_id=cf9ac74c-fefc-4d1c-9fcb-0c5cecd5203d&scope=urn%3a0580150c-7d26-422e-983c-53b211c44a3e&redirect_uri=http%3a%2f%2flocalhost%3a27923%2foauth2%2fcallback&response_type=token

			var request = new HttpRequestMessage(HttpMethod.Get, "https://accounts.matrix42.com/issue/oauth2/authorize?client_id=cf9ac74c-fefc-4d1c-9fcb-0c5cecd5203d&scope=urn%3a0580150c-7d26-422e-983c-53b211c44a3e&redirect_uri=http%3a%2f%2flocalhost%3a27923%2foauth2%2fcallback&response_type=token");
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			//request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

			var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
			response.EnsureSuccessStatusCode();

			var data = await response.Content.ReadAsStringAsync();

			//var user = JObject.Parse(await response.Content.ReadAsStringAsync());

			//context.RunClaimActions(user);
			//return data;
			return null;
		}

		private ClaimsPrincipal ValidateToken(string token)
		{
			var handler = new JwtSecurityTokenHandler();

			string certificatePath = Path.Combine(_hostingEnvironment.WebRootPath, "047162cd-8d52-4241-a6ce-d60339aeda6a_0b65abd1-824e-4f0c-a4f6-234848a95f0b.pem");
			var certificate = new X509Certificate2(certificatePath);

			var validationParameters = new TokenValidationParameters()
			{
				ValidateAudience = false,
				ValidateIssuer = false,
				ValidateLifetime = false,
				IssuerSigningKeyResolver = (t, st, i, p)
					=> new List<SecurityKey> { new X509SecurityKey(certificate) },

				//ValidAudience = "https://my-rp.com",
				//ValidIssuer = "https://my-issuer.com/trust/issuer",
				//RequireExpirationTime = true
			};

			var principal = handler.ValidateToken(token, validationParameters, out SecurityToken securityToken);

			//Debugger.Break();
			return principal;

			//return securityToken.ValidFrom <= DateTime.Now && DateTime.Now <= securityToken.ValidTo;
		}
	}
}