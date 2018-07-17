using System;

namespace DialogueSmith.Entities
{
    [Serializable]
    public class EditorSettingEntity
    {
        public string dialogues_path;
        public string dialoguesmith_path = "dialoguesmith";

        public WindowEntity window = new WindowEntity() {
            height = 50f,
            width = 200f,
            x = 10f,
            y = 10f
        };
    }
}
