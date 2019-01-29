using System.Net;
using RestClient.Net.Interfaces;

namespace RestClient.Net.Requests
{
    public class RequestFactory : IRequestFactory
    {
        public IRequest Create(string uri)
        {
            return new Request(WebRequest.Create(uri));
        }
    }
}
