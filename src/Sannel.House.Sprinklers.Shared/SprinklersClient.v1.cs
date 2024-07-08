using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Sannel.House.Sprinklers.Shared.Dtos.Logs;
using Sannel.House.Sprinklers.Shared.Dtos.Sprinklers;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;
using Sannel.House.Sprinklers.Shared.Messages;

namespace Sannel.House.Sprinklers.Shared;
public partial class SprinklersClient
{
	public V1Class V1 { get; init; }

	public class V1Class
	{
		private readonly SprinklersClient _parent;
		private readonly HubConnection _hubConnection;

		public event Action<StationStartMessage>? StationStart;
		public event Action<StationStopMessage>? StationStop;
		public event Action<StationProgressMessage>? StationProgress;
		public event Action<ZoneUpdateMessage>? ZoneUpdate;

		internal V1Class(SprinklersClient parent)
		{
			_parent = parent;
			var urlBuilder = new UriBuilder(parent._options.HostUri!);
			urlBuilder.Path += "/hub";
			_hubConnection = new HubConnectionBuilder()
				.WithUrl(urlBuilder.Uri)
				.WithAutomaticReconnect()
				.Build();
			SetupEvents();
		}

		private void SetupEvents()
		{
			_hubConnection.On<StationStartMessage>(EventNames.START_MESSAGE, m => StationStart?.Invoke(m));
			_hubConnection.On<StationStopMessage>(EventNames.STOP_MESSAGE, m => StationStop?.Invoke(m));
			_hubConnection.On<StationProgressMessage>(EventNames.PROGRESS_MESSAGE, m => StationProgress?.Invoke(m));
			_hubConnection.On<ZoneUpdateMessage>(EventNames.ZONE_UPDATE_MESSAGE, m => ZoneUpdate?.Invoke(m));
		}

		/// <summary>
		/// Start listening for events from the server.
		/// </summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		public async Task StartListeningAsync()
		{
			await _hubConnection.StartAsync();
		}

		/// <summary>
		/// Get all zone metadata.
		/// </summary>
		/// <returns>A task representing the asynchronous operation with the result containing the zone metadata.</returns>
		public Task<Result<IEnumerable<ZoneInfoDto>>> GetAllZoneMetaDataAsync()
		{
			return _parent.GetAsync<IEnumerable<ZoneInfoDto>>("/api/v1/Zone");
		}

		/// <summary>
		/// Get zone metadata by ID.
		/// </summary>
		/// <param name="id">The ID of the zone.</param>
		/// <returns>A task representing the asynchronous operation with the result containing the zone metadata.</returns>
		public Task<Result<ZoneInfoDto>> GetZoneMetaDataAsync(byte id)
		{
			return _parent.GetAsync<ZoneInfoDto>($"/api/v1/Zone/{id}");
		}

		/// <summary>
		/// Update zone metadata.
		/// </summary>
		/// <param name="zone">The updated zone metadata.</param>
		/// <returns>A task representing the asynchronous operation with the result.</returns>
		public Task<Result> UpdateZoneMetaDataAsync(ZoneInfoDto zone)
		{
			ArgumentNullException.ThrowIfNull(zone);
			return _parent.PutAsync("/api/v1/Zone", zone);
		}

		/// <summary>
		/// Start a zone.
		/// </summary>
		/// <param name="id">The ID of the zone to start.</param>
		/// <param name="length">The duration of the zone run.</param>
		/// <returns>A task representing the asynchronous operation with the result.</returns>
		public Task<Result> StartZoneAsync(byte id, TimeSpan length)
		{
			return _parent.PostAsync($"/api/v1/Sprinklers/Start?zoneId={id}&length={JsonSerializer.Serialize(length)}");
		}

		/// <summary>
		/// Stop all zones.
		/// </summary>
		/// <returns>A task representing the asynchronous operation with the result.</returns>
		public Task<Result> StopAll()
		{
			return _parent.PostAsync("/api/v1/Sprinklers/Stop");
		}

		/// <summary>
		/// Get the status of the sprinklers.
		/// </summary>
		/// <returns>A task representing the asynchronous operation with the result containing the sprinkler status.</returns>
		public Task<Result<StatusDto>> GetStatus()
		{
			return _parent.GetAsync<StatusDto>("/api/v1/Sprinklers/Status");
		}

		/// <summary>
		/// Get the zone runs for a specified range.
		/// </summary>
		/// <param name="start">The start date and time of the range.</param>
		/// <param name="end">The end date and time of the range.</param>
		/// <returns>A task representing the asynchronous operation with the result containing the zone runs.</returns>
		public Task<Result<IEnumerable<ZoneRunDto>>> GetRunsForRangeAsync(DateOnly start, DateOnly end)
			=> GetRunsForRangeAsync(start, end, DateTimeOffset.Now.Offset);

		/// <summary>
		/// Get the zone runs for a specified range.
		/// </summary>
		/// <param name="start">The start date and time of the range.</param>
		/// <param name="end">The end date and time of the range.</param>
		/// <returns>A task representing the asynchronous operation with the result containing the zone runs.</returns>
		public Task<Result<IEnumerable<ZoneRunDto>>> GetRunsForRangeAsync(DateOnly start, DateOnly end, TimeSpan offset)
		{
			return _parent.GetAsync<IEnumerable<ZoneRunDto>>($"/api/v1/Logs/Runs/{start.Year}-{start.Month:00}-{start.Day:00}/{end.Year}-{end.Month:00}-{end.Day:00}?offset={offset}");
		}
	}
}
