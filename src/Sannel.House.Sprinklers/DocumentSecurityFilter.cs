using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sannel.House.Sprinklers;

public class DocumentSecurityFilter : IDocumentFilter
{
	private readonly IConfiguration _configuration;

	public DocumentSecurityFilter(IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);
		_configuration = configuration;
	}

	public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context) => swaggerDoc.Components.SecuritySchemes.Add("AzureAd", new OpenApiSecurityScheme()
	{
		Type = SecuritySchemeType.OAuth2,
		Name = "Auth",
		Flows = new OpenApiOAuthFlows()
		{
			Implicit = new OpenApiOAuthFlow()
			{
				AuthorizationUrl = new Uri($"{_configuration["AzureAd:Instance"]}{_configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),
				TokenUrl = new Uri($"{_configuration["AzureAd:Instance"]}{_configuration["AzureAd:TenantId"]}/oauth2/v2.0/token"),
				Scopes = new Dictionary<string, string>()
				{
					{"api://sannel.house/access", "access" }
				}
			}
		},
	});
}
