using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Shared;
public partial class SprinklersClient
{
	private readonly HttpClient _httpClient;

	public SprinklersClient(HttpClient httpClient)
	{
		ArgumentNullException.ThrowIfNull(httpClient);
		_httpClient = httpClient;
		V1 = new V1Class(httpClient);
	}
}
