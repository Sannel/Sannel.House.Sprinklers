using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Shared;
public class Result
{
	public bool IsSuccess { get; set; }
	public HttpStatusCode StatusCode { get; set; }
}
public class Result<T> : Result
{
	public T? Value { get; set; }
}
