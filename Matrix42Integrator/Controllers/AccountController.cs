using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Matrix42Integrator.Controllers
{
	[Route("[controller]/[action]")]
	public class AccountController : Controller
	{
		private readonly IHostingEnvironment _hostingEnvironment;
		private HttpClient HttpClient { get; } = new HttpClient();

		public AccountController(IHostingEnvironment hostingEnvironment)
		{
			_hostingEnvironment = hostingEnvironment;
		}


		[HttpGet]
		public IActionResult SignIn()
		{
			//HttpContext.SignOutAsync();
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
			//return RedirectToAction("GetUserData");
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

		public async Task<IActionResult> GetUserData()
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "https://accounts.matrix42.com/api/session/profile");
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IjRqTUY1NEpkQnNnZWRVMjJjYllKeXhSa09VNCJ9.eyJhdXRobWV0aG9kIjoiT0F1dGgyIiwiYXV0aF90aW1lIjpbIjIwMTgtMDYtMTJUMTM6MzY6MzkuMjk4WiIsIjYvMTIvMjAxOCAxOjM2OjM5IFBNIl0sIm5hbWVpZCI6ImJvbmRhcmNodWsubS55QGdtYWlsLmNvbSIsInVuaXF1ZV9uYW1lIjoiYm9uZGFyY2h1ay5tLnlAZ21haWwuY29tIiwiZW1haWwiOiJib25kYXJjaHVrLm0ueUBnbWFpbC5jb20iLCJlbnRlcnByaXNlaWQiOiIxOWZmMWYyOC05OTU5LTQxMzYtODlhMi1jZjAyNDdiY2IxNTUiLCJlbnRlcnByaXNlbmFtZSI6Ik15IEJpZyBDb3Jwb3JhdGlvbiIsInJvbGUiOlsiTWF0cml4NDIuTXlXb3Jrc3BhY2UuQ3VzdG9tZXIiLCJNYXRyaXg0Mi5NeVdvcmtzcGFjZS5GUyIsIk1hdHJpeDQyLk1pbmR0b3VjaC5DdXN0b21lcnNNeVdvcmtzcGFjZSIsImVudGVycHJpc2VfMTlmZjFmMjgtOTk1OS00MTM2LTg5YTItY2YwMjQ3YmNiMTU1X21hbmFnZXIiLCJlbnRlcnByaXNlXzE5ZmYxZjI4LTk5NTktNDEzNi04OWEyLWNmMDI0N2JjYjE1NV9Nb2JpbGUgTWFuYWdlbWVudCBBZG1pbmlzdHJhdG9ycyIsImVudGVycHJpc2VfMTlmZjFmMjgtOTk1OS00MTM2LTg5YTItY2YwMjQ3YmNiMTU1X0FsbCB1c2VycyIsImVudGVycHJpc2VfMTlmZjFmMjgtOTk1OS00MTM2LTg5YTItY2YwMjQ3YmNiMTU1X1B1c2ggTm90aWZpY2F0aW9uIEFkbWluaXN0cmF0b3JzIl0sImlkZW50aXR5L2NsYWltcy9maXJzdG5hbWUiOiJNYWtzeW0iLCJpZGVudGl0eS9jbGFpbXMvbGFzdG5hbWUiOiJCb25kYXJjaHVrIiwiaWRlbnRpdHkvY2xhaW1zL2NvbXBhbnkiOiJNeSBCaWcgQ29ycG9yYXRpb24iLCJpZGVudGl0eS9jbGFpbXMvc2FsdXRhdGlvbiI6Ik1yIiwiY3VsdHVyZSI6ImVuIiwiaWRlbnRpdHkvY2xhaW1zL2Vudmlyb25tZW50IjoiaHR0cHM6Ly9hY2NvdW50cy5tYXRyaXg0Mi5jb20iLCJodHRwOi8vaWRlbnRpdHlzZXJ2ZXIudGhpbmt0ZWN0dXJlLmNvbS9jbGFpbXMvY2xpZW50IjoiR2VuZXJhdGVkX2NmOWFjNzRjLWZlZmMtNGQxYy05ZmNiLTBjNWNlY2Q1MjAzZCIsImh0dHA6Ly9pZGVudGl0eXNlcnZlci50aGlua3RlY3R1cmUuY29tL2NsYWltcy9zY29wZSI6InVybjowNTgwMTUwYy03ZDI2LTQyMmUtOTgzYy01M2IyMTFjNDRhM2UiLCJpc3MiOiJodHRwczovL2FjY291bnRzLm1hdHJpeDQyLmNvbSIsImF1ZCI6InVybjowNTgwMTUwYy03ZDI2LTQyMmUtOTgzYy01M2IyMTFjNDRhM2UiLCJleHAiOjE1MjkyNDI1OTksIm5iZiI6MTUyODgxMDU5OX0.aajSI7-DurFEuknDM-GQl0fxk36CJew366qHnnF-uQYx73-9MWkyTSiwux-c533GCp5j57Mu2X2-D2SD453r2BGy7iivopZ-_NAghMd9Q1g31f6rvIp6CrMf41VmeIP1MkuqkkAa9EBBPdohwk89IfvHqWKDcJwRzw356vOp4kb3oHoi9q_NKkiSvPACy1Rlo1tNo-gHvLYsi2EiAuGruDGZc1d5TsvX1c-9eEmJ7NJq1R6wzrbN1VB6qbQW-ZJE7CbgsbC2ZMDz_1pTk5XahY9cSYXBVaJMHdROmBPCd87Fb1Ml44SbZTNLm53GG7RnQ2lXE_zk8m7-sYo3SkAeDw");
			//request.RequestUri.

			//HttpContext.User.Identity
			var response = await HttpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			string accessToken = await HttpContext.GetTokenAsync("access_token");

			var user = JObject.Parse(await response.Content.ReadAsStringAsync());

			//HttpContext.R
			//context.RunClaimActions(user);

			return Ok();
		}
	}
}