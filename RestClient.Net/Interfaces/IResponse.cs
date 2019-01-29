using System.IO;

namespace RestClient.Net.Interfaces
{
    public interface IResponse
    {
        Stream GetResponseStream();
        void Close();
    }
}
