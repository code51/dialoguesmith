using System;
using DialogueSmith.Entities;
using DialogueSmith.Utility;

namespace DialogueSmith.Editor.Node
{
    [Serializable]
    public class NodeDictionary : BaseSerializableDictionary<string, DialogueNode>
    {
        public DialogueEntity GetEntity(string id)
        {
            return data[id].entity;
        }

        public DialogueNode GetNode(string id)
        {
            return data[id];
        }
    }
}
