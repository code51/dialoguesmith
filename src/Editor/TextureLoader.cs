using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DialogueSmith.Editor
{
    public class TextureLoader : Singleton<TextureLoader>
    {
        protected Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();

        public Texture LoadTexture(string path)
        {
            if (textureCache.ContainsKey(path)) {
                if (textureCache[path] == null)
                    NodeEditor.Instance.MissingPath = true;

                return textureCache[path];
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/" + NodeEditor.Instance.Settings.dialoguesmith_path + "/assets/" + path);

            if (texture == null) {
                NodeEditor.Instance.MissingPath = true;
                return null;
            }

            textureCache[path] = texture;

            return texture;
        }
    }
}
