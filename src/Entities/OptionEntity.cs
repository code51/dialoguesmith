using System;

namespace DialogueSmith.Entities
{
    [Serializable]
    public class OptionEntity
    {
        public string dialogue_id { get { return id.Split('.')[0]; } }
        public string id;
        public string text;
        public int dialogue_index;
        //public DialogueEntity dialogue;
    }
}
