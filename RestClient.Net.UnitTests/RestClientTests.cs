using Newtonsoft.Json;
using NUnit.Framework;
using RestClient.Net.Objects;
using RestClient.Net.UnitTests.TestImplementations;

namespace RestClient.Net.UnitTests
{
    [TestFixture()]
    public class RestClientTests
    {
        [TestCase]
        public void RequestingData_Should_ResultInData()
        {
            // arrange
            var client = new TestRestClient();
            var expected = new TestObject() { DecimalData = 1.23M, IntData = 10, StringData = "Some data" };
            client.SetResponse(JsonConvert.SerializeObject(expected));

            // act
            var result = client.Request<TestObject>().Result;

            // assert
            Assert.IsTrue(result.Success);
            Assert.IsTrue(TestHelpers.AreEqual(expected, result.Data));
        }

        [TestCase]
        public void ReceivingInvalidData_Should_ResultInError()
        {
            // arrange
            var client = new TestRestClient();
            client.SetResponse("{\"property\": 123");

            // act
            var result = client.Request<TestObject>().Result;

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
        }

        [TestCase]
        public void ReceivingErrorCode_Should_ResultInError()
        {
            // arrange
            var client = new TestRestClient();
            client.SetErrorWithoutResponse(System.Net.HttpStatusCode.BadRequest, "Invalid request");

            // act
            var result = client.Request<TestObject>().Result;

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
        }

        [TestCase]
        public void ReceivingErrorAndNotParsingError_Should_ResultInFlatError()
        {
            // arrange
            var client = new TestRestClient();
            client.SetErrorWithResponse("{\"errorMessage\": \"Invalid request\", \"errorCode\": 123}", System.Net.HttpStatusCode.BadRequest);

            // act
            var result = client.Request<TestObject>().Result;

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
            Assert.IsTrue(result.Error is ServerError);
            Assert.IsTrue(result.Error.Message.Contains("Invalid request"));
            Assert.IsTrue(result.Error.Message.Contains("123"));
        }

        [TestCase]
        public void ReceivingErrorAndParsingError_Should_ResultInParsedError()
        {
            // arrange
            var client = new ParseErrorTestRestClient();
            client.SetErrorWithResponse("{\"errorMessage\": \"Invalid request\", \"errorCode\": 123}", System.Net.HttpStatusCode.BadRequest);

            // act
            var result = client.Request<TestObject>().Result;

            // assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Error != null);
            Assert.IsTrue(result.Error is ServerError);
            Assert.IsTrue(result.Error.Code == 123);
            Assert.IsTrue(result.Error.Message == "Invalid request");
        }

        [TestCase]
        public void SettingOptions_Should_ResultInOptionsSet()
        {
            // arrange
            // act
            var client = new TestRestClient(new ExchangeOptions("http://test.address.com"));


            // assert
            Assert.IsTrue(client.BaseAddress == "http://test.address.com");
        }
    }
}
