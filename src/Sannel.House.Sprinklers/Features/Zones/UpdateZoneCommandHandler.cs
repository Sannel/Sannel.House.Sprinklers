using Sannel.House.Sprinklers.Features.Messaging;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Features.Notifications;
using Sannel.House.Sprinklers.Mappers;
using Sannel.House.Sprinklers.Shared.Messages;

namespace Sannel.House.Sprinklers.Features.Zones;

public class UpdateZoneCommandHandler : IRequestHandler<UpdateZoneCommand>
{
	private readonly SprinklerDbContext _db;
	private readonly ZoneInfoMapper _mapper;
	private readonly IMessageClient _messageClient;

	public UpdateZoneCommandHandler(SprinklerDbContext db, ZoneInfoMapper mapper, IMessageClient messageClient)
	{
		_db = db;
		_mapper = mapper;
		_messageClient = messageClient;
	}

	public async Task Handle(UpdateZoneCommand request, CancellationToken cancellationToken)
	{
		var zone = await _db.ZoneMetaDatas.FirstOrDefaultAsync(z => z.ZoneId == request.ZoneInfo.ZoneId, cancellationToken);
		if (zone is null)
		{
			zone = new ZoneMetaData { ZoneId = request.ZoneInfo.ZoneId };
			await _db.ZoneMetaDatas.AddAsync(zone, cancellationToken);
		}

		zone.Name = request.ZoneInfo.Name;
		zone.Color = request.ZoneInfo.Color;
		await _db.SaveChangesAsync(cancellationToken);

		await _messageClient.SendZoneUpdateMessageAsync(new ZoneUpdateMessage
		{
			ZoneInfo = _mapper.ModelToDto(zone),
			UpdateTime = DateTimeOffset.Now
		});
	}
}
