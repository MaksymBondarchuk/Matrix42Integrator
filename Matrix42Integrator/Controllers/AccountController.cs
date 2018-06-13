using Matrix42Integrator.SecurityLayer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Matrix42Integrator.Controllers
{
	[Route("[controller]/[action]")]
	public class AccountController : Controller
	{
		[HttpGet]
		public IActionResult SignIn()
		{
			return RedirectToAction("Authorize");
		}

		[HttpGet]
		public IActionResult SignOut()
		{
			HttpContext.SignOutAsync();
			return Redirect("/");
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

			return handler.ValidateToken(token, JwtHandler.GetTokenValidationParameters(), out _);
		}
	}
}