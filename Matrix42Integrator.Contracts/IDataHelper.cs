using System;
using System.Threading.Tasks;

namespace Matrix42Integrator.Contracts
{
    public interface IDataHelper
    {
	    Task<string> GetData(string url, string accessToken);

    }
}
