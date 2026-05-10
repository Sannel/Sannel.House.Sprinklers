namespace Sannel.House.Sprinklers.Features.Notifications;

public class MqttOptions
{
	public string Server { get; set; } = string.Empty;
	public string? Username { get; set; }
	public string? Password { get; set; }
	public bool UseSSL { get; set; }
	public int Port { get; set; } = 1883;
	public List<string>? CertPaths { get; set; }
}
