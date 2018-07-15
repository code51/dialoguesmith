using DialogueSmith.Editors;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DialogueSmith.Managers
{
    public class FileManager : Singleton<FileManager>
    {
        protected Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();

        public Texture LoadTexture(string path)
        {
            if (textureCache.ContainsKey(path))
                return textureCache[path];

            var texture = AssetDatabase.LoadAssetAtPath<Texture>(NodeEditor.DIALOGUESMITH_PATH + "/assets/" + path);

            textureCache[path] = texture;

            return texture;
        }
    }
}
