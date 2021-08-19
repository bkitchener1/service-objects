using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceObjects.ServiceObjectExample
{
    public class ToDoServiceObjectExample : ServiceObjectString
    {
        public ToDoServiceObjectExample()
        {
            this.Domain = "http://jsonplaceholder.typicode.com/";
            this.Endpoint = "todos/{todoId}";

        }

        public string get(string Id)
        {
            this.UrlSegments.Add(new Parameter("todoId", Id));
            Get();
            return this.RestResponse.Content;
        }

        public string GetResponseBody()
        {
            return ResponseBody;
        }
       
    }
}
