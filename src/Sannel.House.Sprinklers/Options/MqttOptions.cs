﻿namespace Sannel.House.Sprinklers.Options;

public class MqttOptions
{
	public string Server { get; set; } = string.Empty;
	public string? Username { get; set; }
	public string? Password { get; set; }
}
