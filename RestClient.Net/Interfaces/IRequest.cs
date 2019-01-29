using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RestClient.Net.Interfaces
{
    public interface IRequest
    {
        Uri Uri { get; }
        WebHeaderCollection Headers { get; set; }
        string Method { get; set; }
        TimeSpan Timeout { get; set; }
        string ContentType { get; set; }
        string Accept { get; set; }
        long ContentLength { get; set; }

        Task<Stream> GetRequestStream();
        Task<IResponse> GetResponse();
    }
}
