using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Matrix42Integrator.Controllers
{
	[Route("[controller]/[action]")]
	public class AccountController : Controller
	{
		private readonly IHostingEnvironment _hostingEnvironment;

		public AccountController(IHostingEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
		}


		[HttpGet]
		public IActionResult Login()
		{
			return RedirectToAction("Authorize");
		}

		public IActionResult Authorize()
		{
			return Redirect("https://accounts.matrix42.com/issue/oauth2/authorize?client_id=cf9ac74c-fefc-4d1c-9fcb-0c5cecd5203d&scope=urn%3a0580150c-7d26-422e-983c-53b211c44a3e&redirect_uri=http%3a%2f%2flocalhost%3a27923%2faccount%2fgettoken&response_type=token");
		}

		// ReSharper disable once InconsistentNaming
		public async Task<IActionResult> GetToken(string access_token)
		{
			ClaimsPrincipal principal = ValidateToken(access_token);

			await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
			return Redirect("/");
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

			return principal;
		}
	}
}