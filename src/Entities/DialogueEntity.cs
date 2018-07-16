using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSmith.Entities
{
    [Serializable]
    public class DialogueEntity
    {
        public string id;
        public List<string> texts = new List<string>() { "" };
        public bool textContinuity = true;
        public WindowEntity window = new WindowEntity();
        public List<OptionEntity> options = new List<OptionEntity>();
        public List<string> variables = new List<string>();
    }
}
