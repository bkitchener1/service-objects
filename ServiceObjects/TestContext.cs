using RestSharp;
using System.Collections.Generic;
using System.Text;

namespace ServiceObjects
{

    /// <summary>
    /// The Context is used to store data.   It provides a mechanism for passing data between step definitions without having to reference a static variable
    /// The Context stores any object using a string key.  If no key is provided it uses the object's type as a string.  
    /// This allows us to save one copy of each type without needing to remember a key
    /// Data is stored in both the Specflow ScenarioContext.Current and a local _context variable so it will support non-specflow implementations 
    /// To use, Context.SaveContext(someObjectVariable)
    /// </summary>
    public class TestContext
    {
        //private variable that contains the actual data
        private static Dictionary<string, object> _context = new Dictionary<string, object>();

        //public variables used globally
        public static RestRequest request = new RestRequest();
        public static ServiceExecutor executor = new ServiceExecutor();

        /// <summary>
        /// Save an object using a string as a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void SaveContext<T>(string key, T obj)
        {
            _context[key] = obj;
        }

        /// <summary>
        /// Save an object using the object's type name as a key
        /// This means only 1 object of each type can be saved
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public static void SaveContext<T>(T obj)
        {
            string key = typeof(T).ToString();
            _context[key] = obj;
        }

        /// <summary>
        /// Get an object out of the context using a string as a key
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="key">The string key</param>
        /// <returns></returns>
        public static T GetContext<T>(string key)
        {
            return (T)_context[key];
        }

        /// <summary>
        /// Gets an object out of the context using the type as a key
        /// Only one object of each type can be saved this way
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetContext<T>()
        {
            string key = typeof(T).ToString();
            return (T)_context[key];
        }

       /// <summary>
       /// Gets all the context objects as a list
       /// </summary>
       /// <returns></returns>
        public static Dictionary<string, object> GetAllContext()
        {
            return _context;
        }

        /// <summary>
        /// Returns a printable string of all context keys and values
        /// </summary>
        /// <returns></returns>
        public static string PrintContext()
        {
            StringBuilder builder = new StringBuilder();
            foreach(var item in _context)
            {
                builder.AppendLine($"{item.Key} : {item.Value.ToString()}");
            }
            return builder.ToString();
        }

        public static void ResetContext()
        {
            _context = new Dictionary<string, object>();
            request = new RestRequest();
            executor = new ServiceExecutor();
    }

        public static bool IsContextPresent(string key)
        {
            try
            {
                var context = _context[key];
                return true;
            }
            catch (KeyNotFoundException e)
            {
                return false;
            }
        }
    }
}
