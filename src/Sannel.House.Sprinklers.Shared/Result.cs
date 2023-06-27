using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Shared;
public class Result<T>
{
	public T? Value { get; set; }
	public bool IsSuccess { get; set; }
	public HttpStatusCode StatusCode { get; set; }
}
