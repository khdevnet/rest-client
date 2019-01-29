using RestClient.Net.Objects;

namespace RestClient.Net.UnitTests
{
    public class TestBaseClient : BaseClient
    {
        public TestBaseClient() : base(new ExchangeOptions(""))
        {
        }

        public CallResult<T> Deserialize<T>(string data)
        {
            return base.Deserialize<T>(data);
        }
    }
}
