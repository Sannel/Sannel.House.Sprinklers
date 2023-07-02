using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Sannel.House.Sprinklers.Shared;
public partial class SprinklersClient
{
	private readonly HttpClient _httpClient;
	private readonly SprinklerClientOptions _options;

	public SprinklersClient(HttpClient httpClient, IOptions<SprinklerClientOptions> options)
	{
		ArgumentNullException.ThrowIfNull(httpClient);
		ArgumentNullException.ThrowIfNull(options);
		_httpClient = httpClient;
		_options = options.Value;
		V1 = new V1Class(this);
	}

	private async Task<Result<T>> GetAsync<T>(string path)
	{
		var result = new Result<T>();

		var builder = new UriBuilder(_options.HostUri!);
		builder.Path += path;

		using var message = new HttpRequestMessage(HttpMethod.Get, builder.Uri);
		var response = await _httpClient.SendAsync(message);

		if(response.IsSuccessStatusCode)
		{
			result.IsSuccess = true;
			result.StatusCode = response.StatusCode;
		}
		else
		{
			result.IsSuccess = false;
			result.StatusCode = response.StatusCode;
			return result;
		}

		var content = await response.Content.ReadAsStringAsync();
		result.Value = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

		return result;
	}

	private async Task<Result> PostAsync(string path)
	{
		var result = new Result();

		var builder = new UriBuilder(_options.HostUri!);
		builder.Path += path;

		using var message = new HttpRequestMessage(HttpMethod.Put, builder.Uri);
		var response = await _httpClient.SendAsync(message);

		if(response.IsSuccessStatusCode)
		{
			result.IsSuccess = true;
			result.StatusCode = response.StatusCode;
		}
		else
		{
			result.IsSuccess = false;
			result.StatusCode = response.StatusCode;
		}

		return result;
	}

	private async Task<Result> PutAsync<T>(string path, T obj)
	{
		var result = new Result();

		var builder = new UriBuilder(_options.HostUri!);
		builder.Path += path;

		using var message = new HttpRequestMessage(HttpMethod.Put, builder.Uri);
		message.Content = JsonContent.Create(obj);
		var response = await _httpClient.SendAsync(message);

		if(response.IsSuccessStatusCode)
		{
			result.IsSuccess = true;
			result.StatusCode = response.StatusCode;
		}
		else
		{
			result.IsSuccess = false;
			result.StatusCode = response.StatusCode;
		}

		return result;
	}
}
