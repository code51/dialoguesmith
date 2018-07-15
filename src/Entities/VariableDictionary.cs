using System;
using DialogueSmith.Utility;

namespace DialogueSmith.Entities
{
    [Serializable]
    public class VariableDictionary : BaseSerializableDictionary<string, string>
    {
        public VariableDictionary Set(string key, string value)
        {
            data[key] = value;

            return this;
        }
    }
}
