using NUnit.Framework;

namespace RestClient.Net.UnitTests
{
    [TestFixture()]
    public class BaseClientTests
    {
        [TestCase]
        public void DeserializingValidJson_Should_GiveSuccessfulResult()
        {
            // arrange
            var client = new TestBaseClient();

            // act
            var result = client.Deserialize<object>("{\"testProperty\": 123}");

            // assert
            Assert.IsTrue(result.Success);
        }

        [TestCase]
        public void DeserializingInvalidJson_Should_GiveErrorResult()
        {
            // arrange
            var client = new TestBaseClient();

            // act
            var result = client.Deserialize<object>("{\"testProperty\": 123");

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
        }
    }
}
