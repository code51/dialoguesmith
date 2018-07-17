using System.IO;
using DialogueSmith.Entities;
using UnityEngine;
using UnityEditor;

namespace DialogueSmith
{
    public class EntityLoader : IEntityLoader
    {
        /// <summary>
        /// path relative to the resources folder
        /// </summary>
        protected string path;

        /// <summary>
        /// The path relative to the resources folder
        /// </summary>
        /// <param name="path"></param>
        public EntityLoader(string path)
        {
            this.path = path;
        }

        public EntityLoader()
        {
            this.path = "";
        }

        public DialogueTreeEntity LoadTree(string name)
        {
            if (path == "")
                throw new System.Exception("The entity loader has no dialogues location available.");

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