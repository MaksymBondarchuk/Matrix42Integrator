using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace OrdersService
{
	public class Startup
	{
		private readonly IHostingEnvironment _hostingEnvironment;

		public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
		{
			Configuration = configuration;
			_hostingEnvironment = hostingEnvironment;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			string certificatePath = Path.Combine(_hostingEnvironment.ContentRootPath, "047162cd-8d52-4241-a6ce-d60339aeda6a_0b65abd1-824e-4f0c-a4f6-234848a95f0b.pem");
			var certificate = new X509Certificate2(certificatePath);

			var validationParameters = new TokenValidationParameters
			{
				ValidateAudience = false,
				ValidateIssuer = false,
				ValidateLifetime = false,
				IssuerSigningKeyResolver = (t, st, i, p)
					=> new List<SecurityKey> { new X509SecurityKey(certificate) },
			};

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = validationParameters;

					//options.Events = new JwtBearerEvents
					//{
					//	OnAuthenticationFailed = context =>
					//	{
					//		Debugger.Break();
					//		return Task.CompletedTask;
					//	},
					//	OnTokenValidated = context =>
					//	{
					//		Debugger.Break();
					//		return Task.CompletedTask;
					//	}
					//};
				});

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseAuthentication();

			app.UseMvc();
		}
	}
}
