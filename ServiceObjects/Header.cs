using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceObjects
{
    public struct Parameter
    {
        public string Key { get; set; }
        public string Value { get; set; }


        public Parameter(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
