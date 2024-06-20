## Prepare our environment
First off we need to install docker on our raspberry pi. I perfer using the script at https://get.docker.com [docs](https://docs.docker.com/engine/install/debian/#install-using-the-convenience-script) 
```sh
curl -sSL https://get.docker.com | sudo sh
```
## Prepare our directories
Lets go ahead and create this directory structures
```sh
sudo mkdir -p /opt/Sannel/House/Sprinklers/config
sudo mkdir -p /opt/Sannel/House/Sprinklers/Data
```
## Configure our service
Navigate to /opt/Sannel/House/Sprinklers/config and create the file appsettings.json. Below is an example configuration that enables HTTPS, sets up our service to connect to Microsoft Entra for authentication, and configures our MQTT server connection.
```json
{
	"Kestrel": {
		"Endpoints": {
			"Https":{
				"Url": "https://*:8443",
				"Certificate":{
					"Path": "/app/app_config/certs/cert.pfx",
					"Password": "@Password1"
				}
			}
		}
	},
	"AllowedHosts": "*",
	"Sprinkler": {
		// My System only has 8 zones so we just configure it for 8
		"Zones": 8
	},
	"AzureAd": {
			"Instance": "https://login.microsoftonline.com/",
			"ClientId": "870818f5-2723-4d4b-b833-b888e65e6cc2",
			"TenantId": "4f7a2a8d-fd6a-46f7-908c-40527c6c7a49",
			"accessTokenAcceptedVersion": "v2.0",
			"Scopes": "access",
			"audience": "api://sannel.house.sprinklers"
	},
	"ApplicationInsights": {
			"ConnectionString": "InstrumentationKey=f31b6cf1-0694-413b-a6ca-a412c147ee23;IngestionEndpoint=https://westus3-1.in.applicationinsights.azur
e.com/"
	},
	"MQTT": {
		"Server": "mqttserver",
		"Username": "sprinklers",
		"Password": "@Password1",
		"UseSSL": true,
		"Port": 8884,
		"CertPaths": ["/app/app_config/certs/ca.crt"]
	}
}
```
Now that the configuration is complete, we can proceed to set up our Compose file. In the example below, our user ID is 1000 and the group ID is 993. Be sure to verify that your GPIO group has the same ID; if not, use the correct ID for your setup.
```yaml
services:
	sprinklers:
		image: sannel/house.sprinklers:latest
		user: "1000:993"
		ports:
		- 8443:8443
		volumes:
		- ./Data:/app/Data
		- ./config:/app/app_config
		devices:
		- /dev/gpiomem
```