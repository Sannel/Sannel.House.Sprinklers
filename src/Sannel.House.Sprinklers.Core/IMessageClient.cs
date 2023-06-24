using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sannel.House.Sprinklers.Shared.Messages;

namespace Sannel.House.Sprinklers.Core;
public interface IMessageClient
{
	Task SendStartMessageAsync(StationStartMessage message);

	Task SendStopMessageAsync(StationStopMessage message);
}
