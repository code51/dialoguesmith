using System.IO;
using DialogueSmith.Entities;
using UnityEngine;

namespace DialogueSmith.Managers
{
    public class EntityManager
    {
        /// <summary>
        /// path relative to Assets folder
        /// </summary>
        protected string path;

        public EntityManager(string path)
        {
            this.path = path;
        }

        public DialogueTreeEntity LoadTree(string name)
        {
            TextAsset text = Resources.Load<TextAsset>(path + "/" + name);

            if (text == null)
                throw new FileNotFoundException("resource " + path + "/" + name + ".json was not found");

            return LoadTree(text);
        }

        public DialogueTreeEntity LoadTree(TextAsset text)
        {
            return JsonUtility.FromJson<DialogueTreeEntity>(text.text);
        }
    }
}
