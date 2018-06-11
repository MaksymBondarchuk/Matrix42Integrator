using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Matrix42Integrator
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = "Matrix42";
			})
			.AddCookie()
			.AddOAuth("Matrix42", options =>
			{
				options.ClientId = Configuration["Matrix42:ClientId"];
				options.ClientSecret = Configuration["Matrix42:ClientSecret"];
				options.Scope.Add(Configuration["Matrix42:Scope"]);
				options.CallbackPath = new PathString("/oauth2/callback");

				options.AuthorizationEndpoint = "https://accounts.matrix42.com/issue/oauth2/authorize";
				options.TokenEndpoint = "https://accounts.matrix42.com/issue/oauth2/token";
				options.UserInformationEndpoint = "https://accounts.matrix42.com/my/account";

				options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
				options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
				options.ClaimActions.MapJsonKey("urn:github:login", "login");
				options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
				options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

				options.Events = new OAuthEvents
				{
					OnCreatingTicket = async context =>
					{
						var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
						request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
						request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

						var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
						response.EnsureSuccessStatusCode();

						var user = JObject.Parse(await response.Content.ReadAsStringAsync());

						context.RunClaimActions(user);
					},
					OnTicketReceived = context =>
					{
						Debugger.Break();
						throw new NotImplementedException();
					}
				};
			});

			//services.AddAuthentication(options =>
			//{
			//	options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			//	options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			//	options.DefaultChallengeScheme = "GitHub";
			//})
			//.AddCookie()
			//.AddOAuth("GitHub", options =>
			//{
			//	options.ClientId = Configuration["GitHub:ClientId"];
			//	options.ClientSecret = Configuration["GitHub:ClientSecret"];
			//	options.CallbackPath = new PathString("/signin-github");

			//	options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
			//	options.TokenEndpoint = "https://github.com/login/oauth/access_token";
			//	options.UserInformationEndpoint = "https://api.github.com/user";

			//	options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
			//	options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
			//	options.ClaimActions.MapJsonKey("urn:github:login", "login");
			//	options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
			//	options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

			//	options.Events = new OAuthEvents
			//	{
			//		OnCreatingTicket = async context =>
			//		{
			//			var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
			//			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			//			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

			//			var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
			//			response.EnsureSuccessStatusCode();

			//			var user = JObject.Parse(await response.Content.ReadAsStringAsync());

			//			context.RunClaimActions(user);
			//		}
			//	};
			//});

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseBrowserLink();
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
			}

			app.UseStaticFiles();

			app.UseAuthentication();

			app.UseMvc();
		}
	}
}
