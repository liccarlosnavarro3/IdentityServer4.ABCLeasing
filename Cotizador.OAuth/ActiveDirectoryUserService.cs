/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using System.Threading.Tasks;
using Thinktecture.IdentityServer.Core;
using Thinktecture.IdentityServer.Core.Models;
using Thinktecture.IdentityServer.Core.Services;

namespace ABCLeasing.OAuth
{
    public class ActiveDirectoryUserService : IUserService
    {
        private const string DOMAIN = "MYDOMAIN";

        public Task<AuthenticateResult> AuthenticateExternalAsync(ExternalIdentity externalUser, SignInMessage message)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }

        public Task<AuthenticateResult> AuthenticateLocalAsync(string username, string password, SignInMessage message)
        {
            try
            {
                using (var pc = new PrincipalContext(ContextType.Domain, DOMAIN))
                {
                    if (pc.ValidateCredentials(username, password))
                    {
                        using (var user = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, username))
                        {
                            if (user != null)
                            {
                                return Task.FromResult(new AuthenticateResult(subject: Guid.NewGuid().ToString(), name: username));
                            }
                        }
                    }

                    // The user name or password is incorrect
                    return Task.FromResult<AuthenticateResult>(null);
                }
            }
            catch
            {
                // Server error
                return Task.FromResult<AuthenticateResult>(null);
            }
        }

        public Task<IEnumerable<Claim>> GetProfileDataAsync(ClaimsPrincipal subject, IEnumerable<string> requestedClaimTypes = null)
        {
            using (var pc = new PrincipalContext(ContextType.Domain, DOMAIN))
            {
                using (var user = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, subject.Identity.Name))
                {
                    if (user != null)
                    {
                        var identity = new ClaimsIdentity();
                        identity.AddClaims(new[]
                        {
                            new Claim(Constants.ClaimTypes.Name, user.DisplayName),
                            new Claim(Constants.ClaimTypes.Email, user.EmailAddress)
                        });

                        if (requestedClaimTypes != null)
                            return Task.FromResult(identity.Claims.Where(x => requestedClaimTypes.Contains(x.Type)));

                        return Task.FromResult(identity.Claims);
                    }
                }
                return Task.FromResult<IEnumerable<Claim>>(null);
            }
        }

        public Task<bool> IsActiveAsync(ClaimsPrincipal subject)
        {
            using (var pc = new PrincipalContext(ContextType.Domain, DOMAIN))
            {
                using (var aduser = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, subject.Identity.Name))
                {
                    return Task.FromResult(aduser != null);
                }
            }
        }

        public Task<AuthenticateResult> PreAuthenticateAsync(SignInMessage message)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }

        public Task SignOutAsync(ClaimsPrincipal subject)
        {
            return Task.FromResult(0);
        }
    }
}*/