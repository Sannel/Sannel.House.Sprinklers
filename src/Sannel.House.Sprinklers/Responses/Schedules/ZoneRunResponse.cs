using Sannel.House.Sprinklers.Core.Schedules.Models;

namespace Sannel.House.Sprinklers.Responses.Schedules;

public class ZoneRunResponse
{
	/// <summary>
	/// The start date and time of the zone run.
	/// </summary>
	public DateTimeOffset StartDateTime { get; set; }

	/// <summary>
	/// The ID of the zone being run.
	/// </summary>
	public byte ZoneId { get; set; }

	/// <summary>
	/// Indicates whether or not the zone has been run.
	/// </summary>
	public bool Ran { get; set; }

	/// <summary>
	/// The duration of the zone run.
	/// </summary>
	public TimeSpan RunLength { get; set; }

	/// <summary>
	/// The date and time zone run was started.
	/// </summary>
	public DateTime? StartedAt { get; set; }

	/// <summary>
	/// Initializes a new instance of the ZoneRunResponse class.
	/// </summary>
	/// <param name="zoneRun">The zone run to copy properties from.</param>
	public ZoneRunResponse(ZoneRun zoneRun)
	{
		this.StartDateTime = new DateTimeOffset(zoneRun.StartDateTime, DateTimeOffset.Now.Offset);
		this.ZoneId = zoneRun.ZoneId;
		this.Ran = zoneRun.Ran;
		this.RunLength = zoneRun.RunLength;
		this.StartedAt = zoneRun.StartedAt;
	}
}
