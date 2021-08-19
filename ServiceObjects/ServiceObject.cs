using Newtonsoft.Json;
using RestSharp;
using System;

namespace ServiceObjects
{
    /// <summary>
    /// Base class for a service object with a request body type and a response body type
    /// </summary>
    /// <typeparam name="Req">The request body type</typeparam>
    /// <typeparam name="Res">The response body type</typeparam>
    public abstract class ServiceObject<Req, Res> : ServiceObjectBase
    {
        //the desereialized response body will appear here after execution
        public Res ResponseObject { get; set; }

        //the request body using a generic
        public new Req BodyObject { get; set; }
        
        //the execution cycle
        public override IRestResponse Execute(Method method) 
        {
            base.BodyObject = this.BodyObject;
            try
            {
                // Build request
                CreateNewRequest(method);

                // Execute
                RestResponse = Execute(RestRequest);

                // Deserialize
                ResponseObject = JsonConvert.DeserializeObject<Res>(Response.Content);

                // Store service object
                TestContext.SaveContext(this);
                return Response;
            }
            catch(Exception e)
            {
                test.Error(e);
                test.Error($"ERROR CANNOT DESERIALIZE RESPONSE FROM {RestRequest.Resource}");
                throw e;
            }
            
        }
        
    }
}
