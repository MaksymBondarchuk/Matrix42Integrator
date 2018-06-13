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

			    var request = new HttpRequestMessage(HttpMethod.Get, "https://accounts.matrix42.com/api/session/profile");
				request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				var response = await HttpClient.SendAsync(request);
				response.EnsureSuccessStatusCode();

				var user = JObject.Parse(await response.Content.ReadAsStringAsync());

			    FirstName = user.SelectToken("FirstName").Value<string>();
			    LastName = user.SelectToken("LastName").Value<string>();
			    Email = user.SelectToken("MailAddress").Value<string>();
			    AvatarUrl = user.SelectToken("AvatarSasUrl").Value<string>();

			    Roles = ((ClaimsIdentity)User.Identity).Claims
				    .Where(c => c.Type == ClaimTypes.Role)
				    .Select(c => c.Value);


				var apiRequest = new HttpRequestMessage(HttpMethod.Get, "http://ordersservice-matrix42integrator.azurewebsites.net//api/orders");
			    apiRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			    apiRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				var apiResponse = await HttpClient.SendAsync(apiRequest);
			    apiResponse.EnsureSuccessStatusCode();

				JArray apiData = JArray.Parse(await apiResponse.Content.ReadAsStringAsync());

			    Orders = apiData.ToObject<List<string>>();
		    }
	    }

	}
}
