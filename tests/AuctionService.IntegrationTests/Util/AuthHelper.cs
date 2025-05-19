using System;
using System.Security.Claims;

namespace AuctionService.IntegrationTests.Util;

public class AuthHelper
{
  public static Dictionary<string, object> GetBearerForUser(string username)
  {
    // Return dictionary with the claim for the given username
    return new Dictionary<string, object> { { ClaimTypes.Name, username } };
  }
}
