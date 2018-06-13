using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			//services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			//	.AddJwtBearer(options =>
			//	{
			//		options.Authority = "{yourAuthorizationServerAddress}";
			//		options.Audience = "{yourAudience}";
			//	});

	    //    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		   //     .AddJwtBearer(options =>
		   //     {
			  //      options.TokenValidationParameters = new TokenValidationParameters
			  //      {
				 //       // Clock skew compensates for server time drift.
				 //       // We recommend 5 minutes or less:
				 //       ClockSkew = TimeSpan.FromMinutes(5),
				 //       // Specify the key used to sign the token:
				 //       //IssuerSigningKey = signingKey,
				 //       RequireSignedTokens = true,
				 //       // Ensure the token hasn't expired:
				 //       RequireExpirationTime = true,
				 //       ValidateLifetime = true,
				 //       // Ensure the token audience matches our audience value (default true):
				 //       ValidateAudience = true,
				 //       ValidAudience = "api://default",
				 //       // Ensure the token was issued by a trusted authorization server (default true):
				 //       ValidateIssuer = true,
				 //       ValidIssuer = "https://accounts.matrix42.com/issue/oauth2/authorize"
					//};
		   //     });

	        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		        .AddJwtBearer(options => {
			        options.TokenValidationParameters =
				        new TokenValidationParameters
				        {
					        ValidateIssuer = false,
					        ValidateAudience = false,
					        ValidateLifetime = false,
					        ValidateIssuerSigningKey = false,

					        //ValidIssuer = "Fiver.Security.Bearer",
					        //ValidAudience = "Fiver.Security.Bearer",
					        //IssuerSigningKey = JwtSecurityKey.Create("fiversecret ")
				        };

			        options.Events = new JwtBearerEvents
			        {
				        OnAuthenticationFailed = context =>
				        {
					        Console.WriteLine("OnAuthenticationFailed: " +
					                          context.Exception.Message);
					        return Task.CompletedTask;
				        },
				        OnTokenValidated = context =>
				        {
					        Console.WriteLine("OnTokenValidated: " +
					                          context.SecurityToken);
					        return Task.CompletedTask;
				        }
			        };
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
