using Newtonsoft.Json;
using RestSharp;
using System;

namespace ServiceObjects
{

    public abstract class ServiceObjectString : ServiceObjectBase {
        public string ResponseBody { get; set; }

        public override IRestResponse Execute(Method method)
        {
            base.Execute(method);
            try
            {
                ResponseBody = RestResponse.Content;
                return RestResponse;
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
