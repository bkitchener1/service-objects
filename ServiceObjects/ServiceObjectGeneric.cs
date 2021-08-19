using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceObjects
{
    public abstract class ServiceObjectGeneric<T> : ServiceObjectBase
    {
        public T ResponseObject { get; set; }

        public override IRestResponse Execute(Method method) 
        {
            base.Execute(method);
            try
            {
                ResponseObject = JsonConvert.DeserializeObject<T>(Response.Content);
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
