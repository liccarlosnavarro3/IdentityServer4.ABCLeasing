using IdentityServer3.AccessTokenValidation;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.Owin.Cors;
using System.Configuration;

[assembly: OwinStartup(typeof(WebService.Startup))]

namespace WebService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // configure web api
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            //Sección para evitar errores Cors
            /*app.AddCors(options =>
            {
                options.AddPolicy(
                  "CorsPolicy",
                  builder => builder.WithOrigins("https://authfacttest.abcleasing.com.mx")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials());
            });
            app.AddAuthentication(IISDefaults.AuthenticationScheme);*/

            // Allow all origins
            app.UseCors(CorsOptions.AllowAll);

            // require authentication for all controllers
            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                //Authority = "https://authfacttest.abcleasing.com.mx",
                Authority = ConfigurationManager.ConnectionStrings["WebApi"].ConnectionString,
                ValidationMode = ValidationMode.ValidationEndpoint,
                RequiredScopes = new[] {
                    "ABCLeasingAPIMobile"
                },
                    //"ABCLeasingAPI" },
            });
            //config.Filters.Add(new AuthorizeAttribute());

            app.UseWebApi(config);

            //UM2GKJEY11
        }
    }
}