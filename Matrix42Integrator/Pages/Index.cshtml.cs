using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace Matrix42Integrator.Pages
{
    public class IndexModel : PageModel
    {

	    public string FirstName { get; set; }

	    public string LastName { get; set; }

	    public string Email { get; set; }

	    public string AvatarUrl { get; set; }

	    public IEnumerable<string> Roles { get; set; }

	    public IReadOnlyList<string> Orders { get; set; }

	    private HttpClient HttpClient { get; } = new HttpClient();

		public async Task OnGetAsync()
	    {
			// Avatar, Email, FirstName, LastName, Roles, Orders
			if (User.Identity.IsAuthenticated)
		    {
				string accessToken = await HttpContext.GetTokenAsync("access_token");

			    var user = JObject.Parse(await GetData("https://accounts.matrix42.com/api/session/profile", accessToken));

			    FirstName = user.SelectToken("FirstName").Value<string>();
			    LastName = user.SelectToken("LastName").Value<string>();
			    Email = user.SelectToken("MailAddress").Value<string>();
			    AvatarUrl = user.SelectToken("AvatarSasUrl").Value<string>();

			    Roles = ((ClaimsIdentity)User.Identity).Claims
				    .Where(c => c.Type == ClaimTypes.Role)
				    .Select(c => c.Value);

				JArray ordersApiData = JArray.Parse(await GetData("http://ordersservice-matrix42integrator.azurewebsites.net//api/orders", accessToken));

			    Orders = ordersApiData.ToObject<List<string>>();
		    }
	    }

	    private async Task<string> GetData(string url, string accessToken)
	    {
		    var request = new HttpRequestMessage(HttpMethod.Get, url);
		    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

		    var response = await HttpClient.SendAsync(request);
		    response.EnsureSuccessStatusCode();

		    return await response.Content.ReadAsStringAsync();
		}
	}
}
