using ServiceObjects.ServiceObjectExample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceObjects.ServiceObjectExample
{
    public class ToDosServiceObject : ServiceObjectList<ToDo>
    {
        public ToDosServiceObject()
        {
            this.Domain = "http://jsonplaceholder.typicode.com/";
            this.Endpoint = "todos";

        }

        public int getUncompleted()
        {
            Get();
            var result = ResponseObjects.First(x => x.completed == false);
            TestContext.SaveContext<int>("UncompletedId", result.userId);
            return result.userId;
        }
    }
}
