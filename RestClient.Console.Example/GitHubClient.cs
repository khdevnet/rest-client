using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestClient.Net.Objects;

namespace RestClientConsoleExample
{
    public class GitHubClient : RestClient.Net.RestClient
    {
        #region fields
        private static ExchangeOptions DefaultOptions => new ExchangeOptions("https://api.github.com");

        private const string EmojisEndpoint = "emojis";
        #endregion

        #region properties
        #endregion

        #region constructor/destructor
        /// <summary>
        /// Create a new instance of BittrexClient using the default options
        /// </summary>
        public GitHubClient() : this(DefaultOptions)
        {
        }

        /// <summary>
        /// Create a new instance of the BittrexClient with the provided options
        /// </summary>
        public GitHubClient(ExchangeOptions options) : base(options)
        {
        }
        #endregion

        #region methods
        #region public

        /// <summary>
        /// Gets information about all available markets
        /// </summary>
        /// <returns>List of markets</returns>
        public CallResult<Dictionary<string, object>> GetEmojis() => GetEmojisAsync().Result;

        /// <summary>
        /// Gets information about all available markets
        /// </summary>
        /// <returns>List of markets</returns>
        public async Task<CallResult<Dictionary<string, object>>> GetEmojisAsync()
        {
            var headers = new Dictionary<string, string>() { { "User-Agent", "Awesome-Octocat-App" } };
            return await Execute<Dictionary<string, object>>(GetUrl(EmojisEndpoint),headers: headers).ConfigureAwait(false);
        }

        #endregion
        #region private

        protected Uri GetUrl(string endpoint)
        {
            var address = BaseAddress;

            var result = $"{address}/{endpoint}";
            return new Uri(result);
        }

        protected override bool IsErrorResponse(JToken data)
        {
            return data["success"] != null && !(bool)data["success"];
        }

        protected override Error ParseErrorResponse(JToken data)
        {
            if (data["message"] == null)
                return new UnknownError("Unknown response from server: " + data);

            return new ServerError((string)data["message"]);
        }

        private async Task<CallResult<T>> Execute<T>(Uri uri, Dictionary<string, object> parameters = null, Dictionary<string, string> headers = null, string method = "GET") where T : class
        {
            return await ExecuteRequest<T>(uri, method, parameters, headers).ConfigureAwait(false);
        }
        #endregion
        #endregion
    }
}
