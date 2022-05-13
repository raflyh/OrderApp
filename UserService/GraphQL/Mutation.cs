using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderApp.Domain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserService.GraphQL
{
    public class Mutation
    {
        public async Task<UserData> RegisterUserAsync(
            RegisterUser input,
            [Service] OrderDbContext context)
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                Fullname = input.Fullname,
                Email = input.Email,
                Username = input.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };
            var memberRole = context.Roles.Where(m => m.Name == "MEMBER").FirstOrDefault();
            if (memberRole == null)
                throw new Exception("Invalid Role");
            var userRole = new UserRole
            {
                RoleId = memberRole.Id,
                UserId = newUser.Id
            };
            newUser.UserRoles.Add(userRole);
            // EF
            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                Fullname = newUser.Fullname
            });
            //using var transaction = context.Database.BeginTransaction();
            //var resp = new UserData();
            //try
            //{
            //    var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            //    if (user != null)
            //    {
            //        throw new Exception("Username already exist");
            //    }
            //    var newUser = new User
            //    {
            //        Email = input.Email,
            //        Username = input.Username,
            //        Fullname = input.Fullname,
            //        Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            //    };
            //    context.Users.Add(newUser);
            //    // EF
            //var memberRole = context.Roles.Where(m => m.Name == "MEMBER").FirstOrDefault();
            //if (memberRole == null)
            //    throw new Exception("Invalid Role");

            //var userRole = new UserRole
            //{
            //    RoleId = memberRole.Id,
            //    UserId = newUser.Id
            //};
            //newUser.UserRoles.Add(userRole);

            //    context.SaveChanges();
            //    await transaction.CommitAsync();

            //    return await Task.FromResult(new UserData
            //    {
            //        Id = newUser.Id,
            //        Username = newUser.Username,
            //        Email = newUser.Email,
            //        Fullname = newUser.Fullname
            //    });
            //}
            //catch (Exception ex)
            //{
            //    transaction.Rollback();
            //}
            //return await Task.FromResult(resp);
        }
        public async Task<UserToken> LoginAsync(
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings, // setting token
            [Service] OrderDbContext context) // EF
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                // generate jwt token
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                // jwt payload
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

                var userRoles = context.UserRoles.Where(o => o.UserId == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.Id == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(5);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims, // jwt payload
                    signingCredentials: credentials // signature
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }
    }
}
