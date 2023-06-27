using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using Sannel.House.Sprinklers.Infrastructure.Options;

namespace Sannel.House.Sprinklers.Infrastructure;
public class MQTTManager : IDisposable
{
	private readonly MqttOptions _options;
	private readonly MqttFactory _factory = new();
	private readonly IMqttClient _client;
	private readonly ILogger _logger;
	private bool _isTryingToConnect = false;

	public MQTTManager(IOptions<MqttOptions> options, ILogger<MQTTManager> logger)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(logger);
		_options = options.Value;
		_client = _factory.CreateMqttClient();
		_logger = logger;

		_client.DisconnectedAsync += disconnectedAsync;
		_client.ConnectedAsync += (args) =>
		{
			_logger.LogInformation("Connected to MQTT Server");
			return Task.CompletedTask;
		};
		_client.ConnectingAsync += (args) =>
		{
			_logger.LogInformation("Connecting to MQTT Server");
			return Task.CompletedTask;
		};
	}

	private async Task disconnectedAsync(MqttClientDisconnectedEventArgs arg)
	{
		if(_isTryingToConnect)
		{
			return;
		}
		_isTryingToConnect = true;

		if (arg.Exception is not null)
		{
			_logger.LogError(arg.Exception, "Disconnected from MQTT Server {reason}", arg.ReasonString);
		}
		else
		{
			_logger.LogWarning("Disconnected from MQTT Server {reason}", arg.ReasonString);
		}

		TimeSpan delay = TimeSpan.FromMicroseconds(250);

		if (arg.ClientWasConnected)
		{
			do
			{
				await connectAsync();
				if (!_client.IsConnected)
				{
					await Task.Delay(delay);
					if (delay.TotalSeconds < 30)
					{
						delay = delay.Add(delay);
					}
				}
			} while (!_client.IsConnected);
		}
		_isTryingToConnect = false;
	}

	private async Task connectAsync()
	{
		try
		{
			var o = new MqttClientOptionsBuilder()
					.WithTcpServer(_options.Server, _options.Port);

			if (!string.IsNullOrWhiteSpace(_options.Username))
			{
				o = o.WithCredentials(_options.Username, _options.Password);
			}

			if(_options.UseSSL)
			{
				o = o.WithTls(p =>
				{
					p.UseTls = true;
					if(_options.CertPaths?.Any() == true)
					{
						p.Certificates = _options.CertPaths.Select(i => new X509Certificate2(i)).ToList();
						p.CertificateValidationHandler += (certContext) =>
						{
							X509Chain chain = new X509Chain();
							chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
							chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
							chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
							chain.ChainPolicy.VerificationTime = DateTime.Now;
							chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);
							foreach (var c in p.Certificates)
							{
								chain.ChainPolicy.CustomTrustStore.Add(c);
							}

							chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;

							// convert provided X509Certificate to X509Certificate2
							var x5092 = new X509Certificate2(certContext.Certificate);

							return chain.Build(x5092);
						};
					}
				});
			}

			await _client.ConnectAsync(o.Build());
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error connecting to MQTT Server. {Server}", _options.Server);
		}
	}

	public Task StartAsync()
	{
		return connectAsync();
	}

	public Task PublishAsync(MqttApplicationMessage message)
	{
		return _client.PublishAsync(message);
	}

	public void Dispose()
	{
		_client.Dispose();
	}
}
