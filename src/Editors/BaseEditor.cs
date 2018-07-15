using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DialogueSmith.Entities;
using DialogueSmith.Helper;
using UnityEditor;
using UnityEngine;

namespace DialogueSmith.Editors
{
    public abstract class BaseEditor : EditorWindow
    {
        public const string DIALOGUESMITH_PATH = "Assets/dialoguesmith";
        public const string SETTING_DIALOGUES_PATH = "dialogues_path";
        public const string SETTING_DIALOGUES_META = "dialogues_meta.json";

        protected Vector2 mousePos;

        public void DialogueManager(Event e)
        {
            try {
                GeneralMenu(e);
            } catch(Exception exception) {
                EditorUtility.DisplayDialog("", exception.Message, "Ok");

                EditorPrefs.DeleteKey(SETTING_DIALOGUES_PATH);
            }
        }

        protected GenericMenu BuildLoadMenu(string prefix, GenericMenu menu, Func<bool> callback)
        {
            GetAllEntries().ForEach(entry => {
                string name = entry.Replace("\\", "/").Replace(EditorPrefs.GetString(SETTING_DIALOGUES_PATH) + "/", "").Replace(".json", "");

                menu.AddItem(new GUIContent(prefix + "Load/" + name), false, delegate {
                    if (callback != null)
                        if (!callback())
                            return;

                    LoadTree(entry);
                });
            });

            return menu;
        }

        protected void GeneralMenu(Event e)
        {
            if (e.type != EventType.ContextClick)
                return;

            GenericMenu menu = new GenericMenu();

            if (EditorPrefs.HasKey(SETTING_DIALOGUES_PATH)) {
                menu.AddItem(new GUIContent("Change Dialogues Path"), false, delegate {
                    string originPath = "Assets";

                    if (Directory.Exists(EditorPrefs.GetString(SETTING_DIALOGUES_PATH)))
                        originPath = EditorPrefs.GetString(SETTING_DIALOGUES_PATH);

                    string path = EditorUtility.OpenFolderPanel("", originPath, "");

                    if (path != "") {
                        EditorPrefs.SetString(SETTING_DIALOGUES_PATH, path);
                    } else {
                        //EditorPrefs.SetString(SETTING_DIALOGUES_PATH, "Assets");
                    }
                });

                menu.AddItem(new GUIContent("New Tree"), false, delegate {
                    InitializeTree();
                    Repaint();
                });

                BuildLoadMenu("", menu, null);

                menu.AddItem(new GUIContent("Sync"), false, delegate {
                    SyncNaming();
                });
            } else {
                menu.AddItem(new GUIContent("Select Dialogues Path"), false, delegate {
                    string path = EditorUtility.OpenFolderPanel("", "Assets", "");
                    EditorPrefs.SetString(SETTING_DIALOGUES_PATH, path);
                });
            }

            menu.ShowAsContext();
        }
        
        protected List<string> GetAllEntries()
        {
            return FileHelper.RecursiveListAllFiles(EditorPrefs.GetString(SETTING_DIALOGUES_PATH)).Where(entry => {
                return entry.IndexOf(".meta") == -1 && entry.IndexOf(".json") > 0;
            }).ToList();
        }

        public void SyncNaming()
        {
            var entries = GetAllEntries();

            int changes = 0;
            entries.ForEach(entry => {
                if (entry.IndexOf(".json") == -1)
                    return;

                string contents = File.ReadAllText(entry);

                DialogueTreeEntity tree = JsonUtility.FromJson<DialogueTreeEntity>(contents);

                string name = entry.Replace("\\", "/").Replace(EditorPrefs.GetString(SETTING_DIALOGUES_PATH) + "/", "").Replace(".json", "");

                if (name != tree.name) {
                    string nm = tree.name;
                    tree.name = name;

                    // save back
                    File.WriteAllText(entry, JsonUtility.ToJson(tree));

                    Debug.Log("Dialogue tree [" + nm + "] has been renamed to [" + name + "]");

                    changes++;
                }
            });

            if (changes == 0)
                Debug.Log("0 Changes..");
        }

        protected abstract void LoadTree(string path);

        protected abstract void InitializeTree();
    }
}
