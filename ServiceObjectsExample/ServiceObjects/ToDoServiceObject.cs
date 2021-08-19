using ServiceObjects.ServiceObjectExample.Models;

namespace ServiceObjects.ServiceObjectExample
{
    public class ToDoServiceObject : ServiceObjectGeneric<ToDo>
    {
        public ToDoServiceObject()
        {
            this.Domain = "http://jsonplaceholder.typicode.com/";
            this.Endpoint = "todos/{todoId}";

        }

        public ToDo GetTodo(string Id)
        {
            this.UrlSegments.Add(new Parameter("todoId", Id));
            Get();
            return ResponseObject;
        }

        public ToDo GetUncompletedToDo(int uncompletedId)
        {
            GetTodo(uncompletedId.ToString());
            return ResponseObject;

        }

        public ToDo GetUncompletedToDo()
        {
            var latestId = TestContext.GetContext<int>("UncompletedId");
            GetTodo(latestId.ToString());
            return ResponseObject;

        }

        public ToDo Complete(ToDo todo)
        {
            todo.completed = true;
            this.BodyObject = todo;
            this.UrlSegments.Add(new Parameter("todoId", todo.id.ToString()));
            Put();
            return ResponseObject;
        }
    }
}
