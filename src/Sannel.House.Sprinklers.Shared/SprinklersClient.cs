using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Shared;
public partial class SprinklersClient
{
	private readonly HttpClient _httpClient;

	public SprinklersClient(HttpClient httpClient)
	{
		ArgumentNullException.ThrowIfNull(httpClient);
		_httpClient = httpClient;
		V1 = new V1Class(this);
	}

	public string PathRoot { get; set; } = "/sprinkler";

	private async Task<Result<T>> GetAsync<T>(string path)
	{
		var result = new Result<T>();
		using var message = new HttpRequestMessage(HttpMethod.Get, path);
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
		using var message = new HttpRequestMessage(HttpMethod.Put, path);
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
		using var message = new HttpRequestMessage(HttpMethod.Put, path);
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
