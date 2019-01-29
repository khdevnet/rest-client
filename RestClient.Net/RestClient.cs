using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RestClient.Net.Interfaces;
using RestClient.Net.Objects;
using RestClient.Net.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RestClient.Net
{
    public abstract class RestClient : BaseClient
    {
        /// <summary>
        /// The factory for creating requests. Used for unit testing
        /// </summary>
        public IRequestFactory RequestFactory { get; set; } = new RequestFactory();

        protected PostParameters postParametersPosition = PostParameters.InBody;
        protected RequestBodyFormat requestBodyFormat = RequestBodyFormat.Json;

        protected TimeSpan RequestTimeout { get; private set; }
        public int TotalRequestsMade { get; private set; }

        protected RestClient(ExchangeOptions exchangeOptions) : base(exchangeOptions)
        {
            RequestTimeout = exchangeOptions.RequestTimeout;
        }


        /// <summary>
        /// Execute a request
        /// </summary>
        /// <typeparam name="T">The expected result type</typeparam>
        /// <param name="uri">The uri to send the request to</param>
        /// <param name="method">The method of the request</param>
        /// <param name="parameters">The parameters of the request</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<T>> ExecuteRequest<T>(Uri uri, string method = Constants.GetMethod, Dictionary<string, object> parameters = null, Dictionary<string, string> headers = null) where T : class
        {

            var request = ConstructRequest(uri, method, parameters, headers);

            var result = await ExecuteRequest(request).ConfigureAwait(false);
            if (!result.Success)
                return new CallResult<T>(null, result.Error);

            var jsonResult = ValidateJson(result.Data);
            if (!jsonResult.Success)
                return new CallResult<T>(null, jsonResult.Error);

            if (IsErrorResponse(jsonResult.Data))
                return new CallResult<T>(null, ParseErrorResponse(jsonResult.Data));

            return Deserialize<T>(jsonResult.Data);
        }

        /// <summary>
        /// Can be overridden to indicate if a response is an error response
        /// </summary>
        /// <param name="data">The received data</param>
        /// <returns>True if error response</returns>
        protected virtual bool IsErrorResponse(JToken data)
        {
            return false;
        }

        /// <summary>
        /// Creates a request object
        /// </summary>
        /// <param name="uri">The uri to send the request to</param>
        /// <param name="method">The method of the request</param>
        /// <param name="parameters">The parameters of the request</param>
        /// <param name="signed">Whether or not the request should be authenticated</param>
        /// <returns></returns>
        protected virtual IRequest ConstructRequest(Uri uri, string method, Dictionary<string, object> parameters, Dictionary<string, string> headers)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            var uriString = uri.ToString();

            if ((method == Constants.GetMethod || method == Constants.DeleteMethod || postParametersPosition == PostParameters.InUri) && parameters?.Any() == true)
                uriString += "?" + parameters.CreateParamString(true);

            var request = RequestFactory.Create(uriString);
            request.ContentType = requestBodyFormat == RequestBodyFormat.Json ? Constants.JsonContentHeader : Constants.FormContentHeader;
            request.Accept = Constants.JsonContentHeader;
            request.Method = method;

            headers = headers == null ? new Dictionary<string, string>() : headers;

            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);

            if ((method == Constants.PostMethod || method == Constants.PutMethod) && postParametersPosition != PostParameters.InUri)
            {
                if (parameters?.Any() == true)
                    WriteParamBody(request, parameters);
                else
                    WriteParamBody(request, "{}");
            }

            return request;
        }

        /// <summary>
        /// Writes the string data of the parameters to the request body stream
        /// </summary>
        /// <param name="request"></param>
        /// <param name="stringData"></param>
        protected virtual void WriteParamBody(IRequest request, string stringData)
        {
            var data = Encoding.UTF8.GetBytes(stringData);
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream().Result)
                stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes the parameters of the request to the request object, either in the query string or the request body
        /// </summary>
        /// <param name="request"></param>
        /// <param name="parameters"></param>
        protected virtual void WriteParamBody(IRequest request, Dictionary<string, object> parameters)
        {
            if (requestBodyFormat == RequestBodyFormat.Json)
            {
                var stringData = JsonConvert.SerializeObject(parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value));
                WriteParamBody(request, stringData);
            }
            else if (requestBodyFormat == RequestBodyFormat.FormData)
            {
                var formData = HttpUtility.ParseQueryString(String.Empty);
                foreach (var kvp in parameters.OrderBy(p => p.Key))
                    formData.Add(kvp.Key, kvp.Value.ToString());
                var stringData = formData.ToString();
                WriteParamBody(request, stringData);
            }
        }

        /// <summary>
        /// Executes the request and returns the string result
        /// </summary>
        /// <param name="request">The request object to execute</param>
        /// <returns></returns>
        private async Task<CallResult<string>> ExecuteRequest(IRequest request)
        {
            var returnedData = "";
            try
            {
                request.Timeout = RequestTimeout;
                TotalRequestsMade++;
                var response = await request.GetResponse().ConfigureAwait(false);
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    returnedData = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                response.Close();
                return new CallResult<string>(returnedData, null);
            }
            catch (WebException we)
            {
                var response = (HttpWebResponse)we.Response;
                try
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        returnedData = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }

                    response.Close();

                    var jsonResult = ValidateJson(returnedData);
                    return !jsonResult.Success ? new CallResult<string>(null, jsonResult.Error) : new CallResult<string>(null, ParseErrorResponse(jsonResult.Data));
                }
                catch (Exception)
                {
                }

                var infoMessage = "No response from server";
                if (response == null)
                {
                    infoMessage += $" | {we.Status} - {we.Message}";
                    return new CallResult<string>(null, new WebError(infoMessage));
                }

                infoMessage = $"Status: {response.StatusCode}-{response.StatusDescription}, Message: {we.Message}";
                response.Close();
                return new CallResult<string>(null, new ServerError(infoMessage));
            }
            catch (Exception e)
            {
                return new CallResult<string>(null, new UnknownError(e.Message + ", data: " + returnedData));
            }
        }

        /// <summary>
        /// Parse an error response from the server. Only used when server returns a status other than Success(200)
        /// </summary>
        /// <param name="error">The string the request returned</param>
        /// <returns></returns>
        protected virtual Error ParseErrorResponse(JToken error)
        {
            return new ServerError(error.ToString());
        }
    }
}
