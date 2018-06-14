using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Matrix42Integrator.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Matrix42Integrator.Pages
{
	public class IndexModel : PageModel
	{

		public string FirstName { get; private set; }

		public string LastName { get; private set; }

		public string Email { get; private set; }

		public string AvatarUrl { get; private set; }

		public IEnumerable<string> Roles { get; private set; }

		public IReadOnlyList<string> Orders { get; private set; }

		public async Task OnGetAsync([FromServices] IDataHelper dataHelper, [FromServices] IConfiguration configuration)
		{
			if (User.Identity.IsAuthenticated)
			{
				string accessToken = await HttpContext.GetTokenAsync("access_token");

				var user = JObject.Parse(await dataHelper.GetData(configuration["Matrix42:ProfileDataEndPoint"], accessToken));

				FirstName = user.SelectToken("FirstName").Value<string>();
				LastName = user.SelectToken("LastName").Value<string>();
				Email = user.SelectToken("MailAddress").Value<string>();
				AvatarUrl = user.SelectToken("AvatarSasUrl").Value<string>();

				Roles = ((ClaimsIdentity)User.Identity).Claims
					.Where(c => c.Type == ClaimTypes.Role)
					.Select(c => c.Value);

				JArray ordersApiData = JArray.Parse(await dataHelper.GetData(configuration["Matrix42:OrdersEndpoint"], accessToken));

				Orders = ordersApiData.ToObject<List<string>>();
			}
		}
	}
}
