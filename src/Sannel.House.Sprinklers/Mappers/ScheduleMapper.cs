using Riok.Mapperly.Abstractions;
using Sannel.House.Sprinklers.Core.Schedules.Models;
using Sannel.House.Sprinklers.Shared.Dtos.Schedules;

namespace Sannel.House.Sprinklers.Mappers;

[Mapper]
public partial class ScheduleMapper
{
	public partial ScheduleProgramDto ModelToDto(ScheduleProgram program);

	public partial StationTimeDto ModelToDto(StationTime stationTime);

	public partial ScheduleProgram DtoToModel(NewScheduleDto newScheduleDto);
	public partial ScheduleProgram DtoToModel(UpdateScheduleDto updateScheduleDto);
}