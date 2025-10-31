using LearningProject.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningProject.RoleMiddleware
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly LearningProjectContext context;

        public ClaimsTransformer(LearningProjectContext _con)
        {
            context = _con;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var CurrentUser = (ClaimsIdentity)principal.Identity;

            if (CurrentUser != null)
            {
                var CurrentUserName = CurrentUser.Name.Replace("MMRMAKITA\\", "");

                var AppUser = context.User.Where(x => x.Name == CurrentUserName).Include(x => x.roluri).ThenInclude(y => y.Claims).FirstOrDefault();
                    

                 if (AppUser != null)
                 {
                     foreach (var x in AppUser.roluri.Claims)
                     {

                       var NewClaim = new Claim(ClaimTypes.GroupSid, x.name);
                        CurrentUser.AddClaim(NewClaim);
                     }
                 }

            }

            return Task.FromResult(principal);
        }


    }
}