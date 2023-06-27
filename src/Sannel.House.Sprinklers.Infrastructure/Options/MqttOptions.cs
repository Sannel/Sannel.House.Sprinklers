namespace Sannel.House.Sprinklers.Infrastructure.Options;

public class MqttOptions
{
	public string Server { get; set; } = string.Empty;
	public string? Username { get; set; }
	public string? Password { get; set; }
	/*"UseSSL": true,
		"Port": 8883,
		"CertPaths": ["app_config/ca.crt"]*/
	public bool UseSSL { get; set; }
	public int Port { get; set; } = 1883;
	public List<string>? CertPaths { get; set; }
}
