using System;
using System.Globalization;
using RestClient.Net.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RestClient.Net
{
    public abstract class BaseClient
    {
        public string BaseAddress { get; private set; }

        private static readonly JsonSerializer defaultSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Culture = CultureInfo.InvariantCulture
        });

        protected BaseClient(ExchangeOptions options)
        {
            BaseAddress = options.BaseAddress;
        }


        /// <summary>
        /// Tries to parse the json data and returns a token
        /// </summary>
        /// <param name="data">The data to parse</param>
        /// <returns></returns>
        protected CallResult<JToken> ValidateJson(string data)
        {
            try
            {
                return new CallResult<JToken>(JToken.Parse(data), null);
            }
            catch (JsonReaderException jre)
            {
                var info = $"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}. Data: {data}";
                return new CallResult<JToken>(null, new DeserializeError(info));
            }
            catch (JsonSerializationException jse)
            {
                var info = $"Deserialize JsonSerializationException: {jse.Message}. Data: {data}";
                return new CallResult<JToken>(null, new DeserializeError(info));
            }
            catch (Exception ex)
            {
                var info = $"Deserialize Unknown Exception: {ex.Message}. Data: {data}";
                return new CallResult<JToken>(null, new DeserializeError(info));
            }
        }

        /// <summary>
        /// Deserialize a string into an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <param name="data">The data to deserialize</param>
        /// <param name="checkObject">Whether or not the parsing should be checked for missing properties (will output data to the logging if log verbosity is Debug)</param>
        /// <param name="serializer">A specific serializer to use</param>
        /// <returns></returns>
        protected virtual CallResult<T> Deserialize<T>(string data, JsonSerializer serializer = null)
        {
            var tokenResult = ValidateJson(data);
            return !tokenResult.Success ? new CallResult<T>(default(T), tokenResult.Error) : Deserialize<T>(tokenResult.Data, serializer);
        }

        /// <summary>
        /// Deserialize a JToken into an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <param name="obj">The data to deserialize</param>
        /// <param name="serializer">A specific serializer to use</param>
        /// <returns></returns>
        protected CallResult<T> Deserialize<T>(JToken obj, JsonSerializer serializer = null)
        {
            if (serializer == null)
                serializer = defaultSerializer;

            try
            {
                return new CallResult<T>(obj.ToObject<T>(serializer), null);
            }
            catch (JsonReaderException jre)
            {
                var info = $"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}. Received data: {obj}";
                return new CallResult<T>(default(T), new DeserializeError(info));
            }
            catch (JsonSerializationException jse)
            {
                var info = $"Deserialize JsonSerializationException: {jse.Message}. Received data: {obj}";
                return new CallResult<T>(default(T), new DeserializeError(info));
            }
            catch (Exception ex)
            {
                var info = $"Deserialize Unknown Exception: {ex.Message}. Received data: {obj}";
                return new CallResult<T>(default(T), new DeserializeError(info));
            }
        }
    }
}
