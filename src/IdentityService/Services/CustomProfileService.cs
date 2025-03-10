using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services;
public class CustomProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        // Get user's user id from subject of the context using usermanager
        var user = await _userManager.GetUserAsync(context.Subject);
        // Get user's existing claims
        var existingClaims = await _userManager.GetClaimsAsync(user);

        /* We need access to the username for stuff like the seller and the
        winner property in the auctionservice, so if we pass it back with the
        token, it's easier to carry out those tasks */
        var claims = new List<Claim>
        {
            new Claim("username", user.UserName),
        };

        context.IssuedClaims.AddRange(claims);
        context.IssuedClaims.Add(existingClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name));
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}