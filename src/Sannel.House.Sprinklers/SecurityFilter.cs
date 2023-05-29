using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sannel.House.Sprinklers;

public class SecurityFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var authAttribute = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>()
			.Select(i => i.Policy).Distinct();

		var req = new OpenApiSecurityRequirement();
		foreach (var p in authAttribute)
		{
			var scheme = new OpenApiSecurityScheme()
			{
				Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = "AzureAd" },
				Type = SecuritySchemeType.OAuth2
			};

			req[scheme] = new List<string>() { p };

			if (req.Count > 0)
			{
				operation.Security = new OpenApiSecurityRequirement[] { req };
			}
			operation.Responses.TryAdd(((int)HttpStatusCode.Forbidden).ToString(), new OpenApiResponse()
			{
				Description = "Forbidden"
			});
			operation.Responses.TryAdd(((int)HttpStatusCode.Unauthorized).ToString(), new OpenApiResponse()
			{
				Description = "Unauthoroized"
			});
		}
	}
}
