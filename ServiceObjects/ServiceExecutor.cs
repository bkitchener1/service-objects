using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.MarkupUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ServiceObjects
{
    /// <summary>
    /// ServiceExecutor takes a RestRequest, executes it, calculates the time, and logs the result
    /// </summary>
    public class ServiceExecutor
    {
        public string Domain;
        protected IRestClient _restClient;
        public ExtentTest test => ExtentManager.GetTest();
        public IRestResponse Response;
        public long ResponseTimeMs;


        public ServiceExecutor()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            Domain = WebConfig.DefaultUrl;
            _restClient = new RestClient(Domain);
        }

        /// <summary>
        /// Parse the response body using jsonpath and return a JToken
        /// https://www.newtonsoft.com/json/help/html/QueryJsonSelectTokenJsonPath.htm
        /// </summary>
        /// <param name="jsonPath">jsonpath string</param>
        /// <returns>JToken selected by the jsonpath</returns>
   
        public JToken SelectToken(string jsonPath)
        {
            JObject o = JObject.Parse(Response.Content);
            JToken token = o.SelectToken(jsonPath);
            return token;
        }

        /// <summary>
        /// Parse the response body and select multiple tokens using jsonpath
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        public IEnumerable<JToken> SelectTokens(string jsonPath)
        {
            JObject o = JObject.Parse(Response.Content);
            IEnumerable<JToken> tokens = o.SelectTokens(jsonPath);
            return tokens;
        }

        /// <summary>
        /// Parse the response body using jsonpath and return a string of the single value
        /// </summary>
        /// <param name="jsonPath">jsonpath string</param>
        /// <returns></returns>
        public string SelectJsonPath(string jsonPath)
        {
            JToken token = SelectToken(jsonPath);
            if (token != null)
            {
                return token.ToString();
            }
            test.Fail("Could not find value matching jsonpath " + jsonPath);
            throw new Exception("Could not find value matching jsonpath " + jsonPath);
        }

        /// <summary>
        /// Parse the response body using jsonpath and return a list of the resulting values
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        public List<string> SelectJsonPaths(string jsonPath)
        {
            JObject o = JObject.Parse(Response.Content);
            IEnumerable<JToken> tokens = o.SelectTokens(jsonPath);
            List<string> results = new List<string>();
            foreach (var token in tokens)
            {
                results.Add(token.ToString());
            }
            return results;
        }

        /// <summary>
        /// Executes a request, logs the results
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IRestResponse Execute(IRestRequest request)
        {
            

            IRestResponse response = null;
            var stopWatch = new Stopwatch();

            try
            {
                //check each item in the context object.  if it's value is a string, lets try to swap out any {tokens} in the request using the key.
                foreach (var ea in TestContext.GetAllContext())
                {
                    try
                    {

                        //the context object can store values of any type.  this swap will only works for strings
                        if (ea.Value.GetType() == typeof(string))
                        {
                            // Swap URL token if necessary
                            var swapToken = "{" + ea.Key + "}";
                            if (request.Resource.Contains(swapToken)) {
                                request.AddUrlSegment(ea.Key, (string)ea.Value);
                            }

                            //check all parameters.  this means headers, body, and query params can be tokenized too
                            foreach (var param in request.Parameters)
                            {
                                if(param.Value.GetType() == typeof(string))
                                {
                                    string body = (string)(param.Value);
                                    string tokenString = "{" + ea.Key + "}";
                                    string newValue = (string)ea.Value;
                                    param.Value = body.Replace(tokenString, newValue);
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        throw;
                    }


                }
                string search = "\\{\\w+\\}";
                string url = _restClient.BuildUri(request).ToString();
                MatchCollection matches = Regex.Matches(url, search);
                foreach (var match in matches)
                {
                    //throw new Exception($"The token {match} was not swapped");
                }

                foreach (var param in request.Parameters)
                {
                    if(param.Value.GetType() == typeof(string))
                    {
                        matches = Regex.Matches((string)param.Value, search);
                        foreach(var match in matches)
                        {
                            throw new Exception($"The token {match} was not swapped");
                        }
                    }

                }

                // Set User Agent if necessary
                try
                {
                    _restClient.UserAgent = TestContext.GetContext<string>("userAgent");
                    //test.Error($"User agent set to {_restClient.UserAgent}.");
                }
                catch
                {
                    //test.Error($"User agent not set. Value: {_restClient.UserAgent}.");
                }
                

                stopWatch.Start();
                response = _restClient.Execute(request);
                stopWatch.Stop();
                ResponseTimeMs = stopWatch.ElapsedMilliseconds;
                Response = response;

            }
            catch (Exception e)
            {
                test.Error(e);
                test.Error($"ERROR CANNOT EXECUTE REQUEST {request.Resource}");
                Console.WriteLine(e);
                LogRequest(request);
                throw;
            }
            
            LogRequest(request);
            LogResponse(response, ResponseTimeMs);

            TestContext.SaveContext(request);
            TestContext.SaveContext(response);
           // ExtentManager.RemoveTest("request");

            return response;
        }

        /// <summary>
        /// Executes the request and returns the deserialized response body
        /// </summary>
        /// <typeparam name="T">The response body type</typeparam>
        /// <param name="request">the rest request to execute</param>
        /// <returns></returns>
        public T ExecuteAndDeserialize<T>(IRestRequest request) 
        {
             try
            {
                Execute(request);
                var returnObject = JsonConvert.DeserializeObject<T>(Response.Content);
                return returnObject;
            }
            catch (Exception e)
            {
                test.Error(e);
                test.Fail($"ERROR CANNOT EXECUTE REQUEST {request.Resource}");
            }
            return default(T);
        }

        /// <summary>
        /// Executes the request and returns the deserialized response body as a list
        /// </summary>
        /// <typeparam name="T">The response body type</typeparam>
        /// <param name="request">the rest request to execute</param>
        /// <returns></returns>
        public List<T> ExecuteAndDeserializeList<T>(IRestRequest request) 
        {
            try
            {
                Execute(request);
                var returnObject = JsonConvert.DeserializeObject<List<T>>(Response.Content);
                return returnObject;
            }
            catch (Exception e)
            {
                test.Error(e);
                test.Error($"ERROR CANNOT EXECUTE REQUEST {request.Resource}");
            }
            return default(List<T>);
        }

        /// <summary>
        /// Logs a request and response
        /// Must be called after the request has been executed
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="durationMs"></param>
        private void LogRequest(IRestRequest request)
        {
            var extentTest = ExtentManager.GetTest();
            string resource = request.Resource;
            string queryParamsString = "";
            string headerString = "";
            string body = "";
            string paramsString = "";
            
            foreach(var param in request.Parameters)
            {
                if (param.Type == ParameterType.QueryString)
                {
                    queryParamsString += param.Name + "=" + param.Value + "&";
                }

                if (param.Type == ParameterType.HttpHeader)
                {
                    headerString += param.Name + "=" + param.Value + "; ";
                }
            
                if (param.Type == ParameterType.RequestBody)
                {
                    try
                    {
                        var bodyType = param.Value.GetType();
                        if (bodyType == typeof(string))
                        {
                            var bodyObj = JsonConvert.DeserializeObject((string) param.Value);
                            body += JsonConvert.SerializeObject(bodyObj, Formatting.Indented);
                        }
                        else
                        {
                            body += JsonConvert.SerializeObject(param.Value, Formatting.Indented);
                        }
                        
                    }
                    catch
                    {
                        // fallback to bytes if not string
                        body = param.Value.ToString();
                    }
                    //TODO: Catch other exceptions
                }

                if(param.Type == ParameterType.GetOrPost)
                {
                    paramsString += param.Name + "=" + param.Value + "&";
                }
            }
            queryParamsString = queryParamsString.TrimEnd('&');
            paramsString = paramsString.TrimEnd('&');

            string method = request.Method.ToString();
            string uri = _restClient.BuildUri(request).ToString();
            test.Info(MarkupHelper.CreateLabel($"{method} {uri}", ExtentColor.Grey));
            Console.WriteLine($"{method} {uri}");
            List<string[]> requestData = new List<string[]>();
            if (headerString != "")
            {
                string[] requestHeaderData = {"Headers", headerString};
                Console.WriteLine($"Headers: {headerString}");
                requestData.Add(requestHeaderData);
            }

            if (queryParamsString != "")
            {
                string[] queryParamData = {"QueryString", queryParamsString};
                Console.WriteLine($"QueryString: {queryParamsString}");
                requestData.Add(queryParamData);
            }

            if (paramsString != "")
            {
                string[] paramsData = {"Params", paramsString};
                Console.WriteLine($"Params: {paramsString}");
                requestData.Add(paramsData);
            }

            if (body != "")
            {
                string[] bodyData = {"Body", body};
                Console.WriteLine($"Body: {body}");
                requestData.Add(bodyData);
            }
            test.Info(MarkupHelper.CreateTable(requestData.ToArray()));
        }

        public void LogResponse(IRestResponse response, long durationMs)
        {
            if (response == null) return;
            string statusCode = response.StatusCode.ToString();

            string headers = "";
            foreach (var header in response.Headers)
            {
                headers += header.Name + "=" + header.Value + "; ";
            }

            string errorMessage = response.ErrorMessage;
            Console.WriteLine($"RESPONSE {durationMs}ms {(int)response.StatusCode}/{response.StatusDescription}");
            ExtentManager.GetTest()
                .Debug($"RESPONSE {durationMs}ms {(int) response.StatusCode}/{response.StatusDescription}");

            List<string[]> responseData = new List<string[]>();
            string[] status = {"StatusCode", $"{(int)response.StatusCode }/{ response.StatusDescription}"};
            responseData.Add(status);

            if (headers!=null)
            {
                string[] headerData = {"Headers", headers};
                //Console.WriteLine($"Headers: {headers}");
                responseData.Add(headerData);
            }

            if (response.Content != null)
            {
                string[] responseContent = {"Content", response.Content};
                Console.WriteLine($"Content: {response.Content}");

                responseData.Add(responseContent);
            }

            if (errorMessage != null)
            {
                string[] errorData = {"Error Message", errorMessage};
                Console.WriteLine($"Error Message: {errorMessage}");

            }

            test.Info(MarkupHelper.CreateTable(responseData.ToArray()));
        }

        public void SetBaseUrl(string url)
        {
            this._restClient.BaseUrl = new Uri(url);
        }

        public void SetUserAgent(string agent)
        {
            _restClient.UserAgent = agent;
        }

        public string GetBaseUrl()
        {
            return this._restClient.BaseUrl.ToString();
        }
    }
}