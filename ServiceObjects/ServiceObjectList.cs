using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace ServiceObjects
{

    public abstract class ServiceObjectList<T> : ServiceObjectBase {
      
  
        public List<T> ResponseObjects { get; set; }

        public override IRestResponse Execute(Method method)
        {
            base.Execute(method);
            try
            {
                ResponseObjects = JsonConvert.DeserializeObject<List<T>>(Response.Content);
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
