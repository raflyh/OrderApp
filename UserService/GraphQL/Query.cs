using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OrderApp.Domain;
using System.Security.Claims;

namespace UserService.GraphQL
{
    public class Query
    {
        [Authorize]
        public async Task<IQueryable<User>> GetUsers([Service] OrderDbContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            // check admin role ?
            var adminRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role && o.Value == "ADMIN").FirstOrDefault();
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user != null)
            {
                if (adminRole != null)
                    return context.Users;
            }
            return new List<User>().AsQueryable();
        }
            
    }
}
