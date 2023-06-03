using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Sannel.House.Sprinklers;

public class UserClaimsTransformation : IClaimsTransformation
{
	private readonly IConfiguration _configuration;

	public UserClaimsTransformation(IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);
		_configuration = configuration;
	}

	public ClaimsPrincipal Transform(ClaimsPrincipal principal)
	{
		var roleClaims = principal.Claims.Where(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).ToList();
		if (roleClaims.Any() && principal.Identity is not null)
		{
			var claimsIdentity = (ClaimsIdentity)principal.Identity;

			// Remove all the roles in the Principal and
			// Add a new Role Claim based on user logic.
			foreach (var existingClaim in roleClaims)
			{
				var value = _configuration.GetGroupById(existingClaim.Value);
				if (!string.IsNullOrWhiteSpace(value))
				{
					claimsIdentity.RemoveClaim(existingClaim);
					claimsIdentity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, value));
				}
			}
		}

		return principal;
	}

	public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
		=> Task.FromResult(Transform(principal));
}
