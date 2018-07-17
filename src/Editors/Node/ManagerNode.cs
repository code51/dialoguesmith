using DialogueSmith.Entities;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DialogueSmith.Editors.Node
{
    public class ManagerNode : BaseNode
    {
        public override string Title { get { return "Preferences"; } }
        public EditorSettingEntity entity;

        public ManagerNode(EditorSettingEntity entity)
        {
            this.entity = entity;
            this.Window = new Rect(entity.window.x, entity.window.y, entity.window.width, entity.window.height);
        }

        public override void DrawUpdate(BaseEditor editor)
        {
            base.DrawUpdate(editor);

            AddAction(new Rect(110f, 22f, 13f, 13f), "folder.png", () => {
                GUI.FocusControl(null);
                //string currentDir = Path.GetDirectoryName(Environment.CurrentDirectory).Replace("\\", "/");

                string path = EditorUtility.OpenFolderPanel("", "Assets", "").Replace("\\", "/");

                if (path == "" || path == null)
                    return;

                string assetPath = Application.dataPath.Replace("\\", "");

                if (!path.Contains(assetPath)) {
                    EditorUtility.DisplayDialog("", "Selected path must be under /Assets folder", "Ok");
                    return;
                }

                path = path.Replace(assetPath, "").TrimStart('/');
                entity.dialogues_path = path;
                editor.SavePreferences();
            });


            EditorGUILayout.LabelField(new GUIContent("Dialogues Path", "The path to where the dialogues will be saved. The dialogue path will correspond the to name of the dialogue." +
                "\n\n" +
                "This path is relative to the project's /Assets folder."), EditorStyles.boldLabel);
            GUILayout.Label(String.IsNullOrEmpty(entity.dialogues_path) ? "-" : entity.dialogues_path);

            //entity.dialogues_path = TextInput("Dialogues path", entity.dialogues_path, "The path to where the dialogues will be saved. The dialogue path will correspond the to name of the dialogue." +
            //    "\n\n" +
            //    "This path is relative to the project's /Assets folder.");

            //if (GUILayout.Button("Save")) {
            //    var contents = JsonUtility.ToJson(entity);

            //    string path = Application.dataPath + "/dialoguesmith-prefs.json";

            //    File.WriteAllText(path, contents);

            //    Debug.Log("Preferences saved..");
            //}

            if (!String.IsNullOrEmpty(entity.dialogues_path))
            if (GUILayout.Button(new GUIContent("Sync Tree Names", "When there's a manual changes to file name / location inside the path, you can use this to synchronize the tree names accordingly."))) {
                editor.SyncNaming();
            }
        }
    }
}
