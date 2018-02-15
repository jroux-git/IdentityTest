using IdentityTest.Data;
using IdentityTest.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Internal.Networking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityTest
{
    public class UserClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        private readonly ApplicationDbContext _authDbContext;

        public UserClaimsFactory(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> optionsAccessor, ApplicationDbContext authDbContext)
            : base(userManager, roleManager, optionsAccessor)
        {
            _authDbContext = authDbContext;
        }

        public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            ClaimsPrincipal principal = await base.CreateAsync(user);
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)principal.Identity;
            
            claimsIdentity.AddClaims(new[]
            {
                new Claim("FirstName", "Jean"),
                new Claim("LastName", "Roux"),
            });

            return principal;
        }

        private RelationalDataReader ExecuteSqlQuery(DatabaseFacade database, string sql, params object[] parameters)
        {
            IConcurrencyDetector concurrencyDetector = database.GetService<IConcurrencyDetector>();

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = database
                    .GetService<IRawSqlCommandBuilder>()
                    .Build(sql, parameters);

                return rawSqlCommand
                    .RelationalCommand
                    .ExecuteReader(
                        database.GetService<IRelationalConnection>(),
                        parameterValues: rawSqlCommand.ParameterValues);
            }
        }
    }
}
