using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceObjects
{
    /// <summary>
    /// Base class all other service object base classes inherit from
    /// </summary>
    public abstract class ServiceObjectBase : ServiceExecutor {
        public string Domain { get; set; }
        public string Endpoint { get; set; }
        public object BodyObject { get; set; }
        public string BodyString { get; set; }
        public bool XmlData { get; set; }
        public Method Method { get; set; }
        public IRestResponse RestResponse { get; set; }
        public IRestRequest RestRequest { get; set; }
        public List<Parameter> Headers = new List<Parameter>();
        public List<Parameter> QueryParameters = new List<Parameter>();
        public List<Parameter> UrlSegments = new List<Parameter>();
        public List<Parameter> Parameters = new List<Parameter>();


        private string Url
        {
            get
            {
                return Domain + Endpoint;
            }
        }

        /// <summary>
        /// Sets the request body as a string
        /// </summary>
        /// <param name="body"></param>
        public void SetBodyFromString(string body)
        {
            this.BodyString = body;
        }

        /// <summary>
        /// Executes the request
        /// </summary>
        /// <param name="method">http verb</param>
        /// <returns>RestResponse</returns>
        public virtual IRestResponse Execute(Method method)
        {
            CreateNewRequest(method);
            RestResponse = Execute(RestRequest);
            return RestResponse;
        }

        /// <summary>
        /// Create a new RestRequest from the properties of the service object
        /// </summary>
        /// <param name="method"></param>
        protected void CreateNewRequest(Method method)
        {
            if (Domain != null)
            {
                _restClient.BaseUrl = new Uri(Domain);
            }
            RestRequest = new RestRequest(Url);
            RestRequest.Resource = Endpoint;

            try
            {
                _restClient.UserAgent = TestContext.GetContext<string>("userAgent");
                //test.Info($"UserAgent set to {_restClient.UserAgent} in Context.");
            }
            catch
            {
                //test.Info($"User agent not set. Value: {_restClient.UserAgent}.");
            }


            if (XmlData)
            {
                RestRequest.RequestFormat = DataFormat.Xml;
                if (BodyObject != null)
                {
                    RestRequest.AddBody(BodyObject);
                }
                if (BodyString != null)
                {
                    RestRequest.AddParameter("application/xml", BodyString, ParameterType.RequestBody);
                }
            }
            else
            {
                RestRequest.RequestFormat = DataFormat.Json;
                if (BodyObject != null)
                {
                    RestRequest.AddJsonBody(BodyObject);
                }
                if (BodyString != null)
                {
                    RestRequest.AddParameter("application/json", BodyString, ParameterType.RequestBody);
                }
            }
            
            this.Method = method;
            RestRequest.Method = this.Method;
   
            foreach (var header in Headers)
            {
                RestRequest.AddHeader(header.Key, header.Value);
            }
            foreach (var qp in QueryParameters)
            {
                RestRequest.AddQueryParameter(qp.Key, qp.Value);
            }
            foreach (var segment in UrlSegments)
            {
                RestRequest.AddUrlSegment(segment.Key, segment.Value);
            }
            foreach (var param in Parameters)
            {
                RestRequest.AddOrUpdateParameter(param.Key, param.Value);
            }
        }

        /// <summary>
        /// Executes a Get Request
        /// </summary>
        public virtual void Get()
        {
            Execute(Method.GET);
        }

        /// <summary>
        /// Executes a Post request
        /// </summary>
        public virtual void Post()
        {
            Execute(Method.POST);
        }

        /// <summary>
        /// Executes a Put request
        /// </summary>
        public virtual void Put()
        {
            Execute(Method.PUT);
        }

        /// <summary>
        /// Executes a patch request
        /// </summary>
        public virtual void Patch()
        {
            Execute(Method.PATCH);
        }

        /// <summary>
        /// Executes a Delte request
        /// </summary>
        public virtual void Delete()
        {
            Execute(Method.DELETE);
        }
    }

}
