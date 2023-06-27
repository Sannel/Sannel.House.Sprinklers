using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Shared;
public partial class SprinklersClient
{
	public V1Class V1 { get; init; }

	public class V1Class
	{
		private readonly HttpClient _httpClient;
		internal V1Class(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		public async Task<Result<IEnumerable<ZoneInfoDto>>> GetAllZoneMetaDataAsync()
		{
			var result = new Result<IEnumerable<ZoneInfoDto>>();
			var response = await _httpClient.GetAsync("/sprinklers/api/v1/ZoneMetaData");

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
			result.Value = JsonSerializer.Deserialize<IEnumerable<ZoneInfoDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

			return result;
		}
	}
}
