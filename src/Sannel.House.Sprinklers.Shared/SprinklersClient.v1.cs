using Microsoft.AspNetCore.SignalR.Client;
using Sannel.House.Sprinklers.Shared.Dtos.Logs;
using Sannel.House.Sprinklers.Shared.Dtos.Schedules;
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

		public async Task StartListeningAsync()
		{
			await _hubConnection.StartAsync();
		}

		public Task<Result<IEnumerable<ZoneInfoDto>>> GetAllZoneMetaDataAsync()
		{
			return _parent.GetAsync<IEnumerable<ZoneInfoDto>>("/api/v1/Zone");
		}

		public Task<Result<ZoneInfoDto>> GetZoneMetaDataAsync(byte id)
		{
			return _parent.GetAsync<ZoneInfoDto>($"/api/v1/Zone/{id}");
		}

		public Task<Result> UpdateZoneMetaDataAsync(ZoneInfoDto zone)
		{
			ArgumentNullException.ThrowIfNull(zone);
			return _parent.PutAsync("/api/v1/Zone", zone);
		}

		public Task<Result> EnqueueZonesAsync(IEnumerable<ZoneStartRequestDto> zones)
		{
			ArgumentNullException.ThrowIfNull(zones);
			return _parent.PostAsync("/api/v1/Sprinklers/Start", zones);
		}

		public Task<Result> StopAll()
		{
			return _parent.PostAsync("/api/v1/Sprinklers/Stop");
		}

		public Task<Result<StatusDto>> GetStatus()
		{
			return _parent.GetAsync<StatusDto>("/api/v1/Sprinklers/Status");
		}

		public Task<Result<IEnumerable<ScheduleProgramDto>>> GetAllSchedulesAsync()
			=> _parent.GetAsync<IEnumerable<ScheduleProgramDto>>("/api/v1/Schedule");

		public Task<Result<ScheduleProgramDto>> GetScheduleAsync(Guid id)
			=> _parent.GetAsync<ScheduleProgramDto>($"/api/v1/Schedule/{id}");

		public Task<Result<Guid>> CreateScheduleAsync(NewScheduleDto schedule)
		{
			ArgumentNullException.ThrowIfNull(schedule);
			return _parent.PostAsync<NewScheduleDto, Guid>("/api/v1/Schedule", schedule);
		}

		public Task<Result> UpdateScheduleAsync(UpdateScheduleDto schedule)
		{
			ArgumentNullException.ThrowIfNull(schedule);
			return _parent.PutAsync("/api/v1/Schedule", schedule);
		}

		public Task<Result> UpdateScheduleStatusAsync(Guid scheduleId, bool isEnabled)
			=> _parent.PutAsync($"/api/v1/Schedule/{scheduleId}/{isEnabled}");

		public Task<Result<IEnumerable<ZoneRunDto>>> GetRunHistoryAsync(DateOnly from, DateOnly to)
			=> _parent.GetAsync<IEnumerable<ZoneRunDto>>($"/api/v1/Log/Runs/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}");
	}
}
