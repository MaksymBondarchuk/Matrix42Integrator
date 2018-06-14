using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Matrix42Integrator.Contracts;

namespace Matrix42Integrator.BusinessLayer
{
    public class DataHelper: IDataHelper
	{
		public async Task<string> GetData(string url, string accessToken)
	    {
		    var request = new HttpRequestMessage(HttpMethod.Get, url);
		    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

		    var httpClient = new HttpClient();
			var response = await httpClient.SendAsync(request);
		    response.EnsureSuccessStatusCode();

		    return await response.Content.ReadAsStringAsync();
	    }
	}
}
