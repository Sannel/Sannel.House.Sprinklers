using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Shared;
public class SprinklerClientOptions
{
	/// <summary>
	/// The root of the sprinkler service
	/// </summary>
	[Required]
	public Uri? HostUri { get; set; }
}
