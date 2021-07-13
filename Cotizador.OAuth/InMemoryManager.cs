using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Resources;
using IdentityServer3.Core.Services.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;


namespace ABCLeasing.OAuth
{
    public class InMemoryManager
    {
        public List<InMemoryUser> GetUsers()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Subject = "1",
                    Username = "Admin",
                    Password = "abcadmin",
                    Claims = new[]
                    {
                         new Claim("username", "Admin"),
                         new Claim("name", "Carlos Hugo"),
                         new Claim("img", "")
                    },
                    Enabled = true,
                }
            };
        }

        public IEnumerable<Scope> GetScopes()
        {
            return new[]
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                new Scope
                {
                    Name = "ABCLeasingAPI",
                    DisplayName = "Scope ABCLeasing Master",
                    IncludeAllClaimsForUser = true
                },
                new Scope
                {
                    Name = "ABCLeasingAPIMobile",
                    DisplayName = "Scope ABCLeasing Mobile",
                    IncludeAllClaimsForUser = true,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim("NoCliente", alwaysInclude: true)
                    }
                },
            };
        }


        public IEnumerable<Client> GetClients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "ABCLeasing",
                    Enabled = true,
                    AccessTokenType = AccessTokenType.Reference,
                    ClientSecrets = new List<Secret>
                    {
                         new Secret("785BFB36-9A18-4D29-94A5-18FE176F2E6E".Sha256())
                    },
                    ClientName = "ABCLeasing Master 1.0",
                    Flow = Flows.ResourceOwner,
                    AllowAccessToAllScopes = true,
                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        "ABCLeasingAPI"
                    }
                },
                new Client
                {
                    ClientId = "ABCLeasingMobile",
                    Enabled = true,
                    AccessTokenType = AccessTokenType.Reference,
                    ClientSecrets = new List<Secret>
                    {
                         new Secret("896C0C47-AB29-5E3A-A5B6-290F28703F7F".Sha256())
                    },
                    ClientName = "ABCLeasing Mobile 1.0",
                    Flow = Flows.ResourceOwner,
                    AllowAccessToAllScopes = true,
                    AllowedScopes = new List<string>
                    {
                        Constants.StandardScopes.OpenId,
                        Constants.StandardScopes.Profile,
                        "ABCLeasingAPIMobile",
                        "NoCliente"
                    }
                }
            };
        }
    }
}