
using NUnit.Framework;
using ServiceObjects;
using ServiceObjects.ServiceObjectExample;

namespace ServiceObjectsExample
{
    public class ExcampleTest : ServiceTestBase
    {
        ToDoServiceObject todos = new ToDoServiceObject();

        [Test]
        public void TestCompleteTodo()
        {
           var todo = todos.GetTodo("1");
           var response = todos.Complete(todo);
            Assert.AreEqual(true, response.completed,"ToDo was not completed as expected");
        }
    }
}
