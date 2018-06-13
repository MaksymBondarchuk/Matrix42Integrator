using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace Matrix42Integrator.SecurityLayer
{
    public static class JwtHandler
    {
	    public static TokenValidationParameters GetTokenValidationParameters()
	    {
			return new TokenValidationParameters
			{
				ValidateAudience = false,
				ValidateIssuer = false,
				ValidateLifetime = false,
				IssuerSigningKeyResolver = (t, st, i, p)
					=> new List<SecurityKey> { new X509SecurityKey(GetCertificate()) },
			};
		}

	    private static X509Certificate2 GetCertificate()
	    {
		    return new X509Certificate2(GetCertificatePath());
	    }

	    private static string GetCertificatePath()
	    {
		    string codeBase = Assembly.GetExecutingAssembly().CodeBase;
		    var uri = new UriBuilder(codeBase);
		    string path = Uri.UnescapeDataString(uri.Path);
		    var assemblypath = Path.GetDirectoryName(path);

		    return Path.Combine(assemblypath, "Certificates", "047162cd-8d52-4241-a6ce-d60339aeda6a_0b65abd1-824e-4f0c-a4f6-234848a95f0b.pem");
		}
	}
}
