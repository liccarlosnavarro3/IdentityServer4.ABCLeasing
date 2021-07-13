using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(ABCLeasing.OAuth.Startup))]

namespace ABCLeasing.OAuth
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Allow all origins
            //config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            app.UseCors(CorsOptions.AllowAll);

            //EntityFrameworkServiceOptions entityFrameworkOptions = new EntityFrameworkServiceOptions
            //{
            //    ConnectionString = ConfigurationManager.ConnectionStrings["ABCLeasingABC"].ConnectionString
            //};

            InMemoryManager inMemoryManager = new InMemoryManager();
            //SetupClients(inMemoryManager.GetClients(), entityFrameworkOptions);
            //SetupScopes(inMemoryManager.GetScopes(), entityFrameworkOptions);

            IdentityServerServiceFactory factory = new IdentityServerServiceFactory()
            //.UseInMemoryUsers(inMemoryManager.GetUsers())
            .UseInMemoryScopes(inMemoryManager.GetScopes())
            .UseInMemoryClients(inMemoryManager.GetClients());

            //factory.RegisterConfigurationServices(entityFrameworkOptions);
            //factory.RegisterOperationalServices(entityFrameworkOptions);
            
            //Intento de regresar Token de acceso con NoCliente
            //factory.UserService = new Registration<IUserService>(typeof(AbcApiUserService));

            //Respaldo que regresa solo el token
            factory.UserService = new Registration<IUserService>(typeof(ABCLeasingUserService));
            
            //factory.Register(new Registration<IUserService>)

            IdentityServerOptions options = new IdentityServerOptions
            {
                RequireSsl = false,
                Factory = factory
            };
            app.UseIdentityServer(options);

        }

        //public void SetupClients(IEnumerable<Client> clients, EntityFrameworkServiceOptions options)
        //{
        //    using (var context = new ClientConfigurationDbContext(options.ConnectionString, options.Schema))
        //    {
        //        if (context.Clients.Any()) return;

        //        foreach (var client in clients)
        //        {
        //            context.Clients.Add(client.ToEntity());
        //        }
        //        context.SaveChanges();
        //    }

        //}

        //public void SetupScopes(IEnumerable<Scope> scopes, EntityFrameworkServiceOptions options)
        //{
        //    using (var context = new ScopeConfigurationDbContext(options.ConnectionString,options.Schema))
        //    {
        //        if (context.Scopes.Any()) return;

        //        foreach (var scope in scopes)
        //        {
        //            context.Scopes.Add(scope.ToEntity());
        //        }
        //        context.SaveChanges();
        //    }
        //}
    }
}
