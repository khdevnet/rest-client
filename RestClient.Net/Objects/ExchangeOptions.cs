using System;

namespace RestClient.Net.Objects
{
    public class ExchangeOptions
    {
        public ExchangeOptions(string baseAddress, TimeSpan requestTimeout = default(TimeSpan))
        {
            BaseAddress = baseAddress;
            RequestTimeout = requestTimeout == default(TimeSpan) ? TimeSpan.FromSeconds(30) : requestTimeout;
        }

        /// <summary>
        /// The time the server has to respond to a request before timing out
        /// </summary>
        public TimeSpan RequestTimeout { get; set; }

        /// <summary>
        /// The base address of the client
        /// </summary>
        public string BaseAddress { get; }
    }
}
