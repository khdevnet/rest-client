using System.IO;
using System.Net;
using RestClient.Net.Interfaces;

namespace RestClient.Net.Requests
{
    public class Response : IResponse
    {
        private readonly WebResponse response;

        public Response(WebResponse response)
        {
            this.response = response;
        }

        public Stream GetResponseStream()
        {
            return response.GetResponseStream();
        }

        public void Close()
        {
            response.Close();
        }
    }
}
