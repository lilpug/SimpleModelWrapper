using System.Net;

namespace SimpleModelWrapper.Core
{
    public class ModelResult
    {
        public object Response { get; set; }
        public HttpStatusCode? Status { get; set; }
    }
}