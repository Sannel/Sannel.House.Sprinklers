using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Sannel.House.Sprinklers.Shared.Dtos.Sprinklers;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Shared;
public partial class SprinklersClient
{
	public V1Class V1 { get; init; }

	public class V1Class
	{
		private readonly SprinklersClient _parent;
		internal V1Class(SprinklersClient parent)
		{
			_parent = parent;
		}

		public Task<Result<IEnumerable<ZoneInfoDto>>> GetAllZoneMetaDataAsync()
		{
			return _parent.GetAsync<IEnumerable<ZoneInfoDto>>($"{_parent.PathRoot}/api/v1/Zone");
		}

		public Task<Result<ZoneInfoDto>> GetZoneMetaDataAsync(byte id)
		{
			return _parent.GetAsync<ZoneInfoDto>($"{_parent.PathRoot}/api/v1/Zone/{id}");
		}

		public Task<Result> UpdateZoneMetaDataAsync( ZoneInfoDto zone)
		{
			ArgumentNullException.ThrowIfNull(zone);
			return _parent.PutAsync($"{_parent.PathRoot}/api/v1/Zone", zone);
		}

		public Task<Result> StartZoneAsync(byte id, TimeSpan length)
		{
			return _parent.PostAsync($"{_parent.PathRoot}/api/v1/Sprinklers/Start?zoneId={id}&length={JsonSerializer.Serialize(length)}");
		}

		public Task<Result> StopAll()
		{
			return _parent.PostAsync($"{_parent.PathRoot}/api/v1/Sprinklers/Stop");
		}

		public Task<Result<StatusDto>> GetStatus()
		{
			return _parent.GetAsync<StatusDto>($"{_parent.PathRoot}/api/v1/Sprinklers/Status");
		}
	}
}
