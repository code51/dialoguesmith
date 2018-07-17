using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DialogueSmith.Entities;
using DialogueSmith.Helper;
using UnityEditor;
using UnityEngine;
using DialogueSmith.Editor.Node;

namespace DialogueSmith.Editor
{
    public abstract class BaseEditor : EditorWindow
    {
        public const string DIALOGUESMITH_PATH = "Assets/dialoguesmith";
        public const string SETTING_DIALOGUES_META = "dialogues_meta.json";

        public ManagerNode ManagerNode;
        public EditorSettingEntity Settings;

        protected Vector2 mousePos;

        public void DialogueManager(Event e)
        {
            if (ManagerNode == null)
                ManagerNode = new ManagerNode(Settings);

            GeneralMenu(e);
            DrawUI(e);
            ManagerMouseClick(e);
        }

        protected void ManagerMouseClick(Event e)
        {
            if (!(e.type == EventType.MouseDown && e.button == 0))
                return;

            ManagerNode.ClickedUpdate(mousePos);
        }

        protected void DrawUI(Event e)
        {
            BeginWindows();

            int i = 0;

            ManagerNode.Window = GUILayout.Window(i, ManagerNode.Window, delegate {
                ManagerNode.DrawUpdate(this);
            }, ManagerNode.Title);

            EndWindows();
        }

        protected GenericMenu BuildLoadMenu(string prefix, GenericMenu menu, Func<bool> callback)
        {
            List<string> categories = new List<string>();

            menu.AddItem(new GUIContent(prefix + "Tree/Create New..."), false, delegate {
                if (callback != null)
                    if (!callback())
                        return;

                InitializeTree();
                Repaint();
            });

            GetAllEntries().ForEach(entry => {
                string name = entry.Replace("\\", "/").Replace(Application.dataPath + "/" + Settings.dialogues_path + "/", "").Replace(".json", "");

                List<string> segments = name.Split('/').ToList();
                segments.Remove(name.Split('/')[name.Split('/').Length - 1]);

                string category = String.Join("/", segments.ToArray());

                if (!categories.Contains(category)) {
                    menu.AddItem(new GUIContent(prefix + "Tree/" + category + "/Create New..."), false, delegate {
                        if (callback != null)
                            if (!callback())
                                return;

                        InitializeTree(category);
                        Repaint();
                    });
                }

                menu.AddItem(new GUIContent(prefix + "Tree/" + name), false, delegate {
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

            if (!String.IsNullOrEmpty(Settings.dialogues_path)) {
                

                BuildLoadMenu("", menu, null);
            }

            menu.ShowAsContext();
        }

        protected List<string> GetAllEntries()
        {
            return FileHelper.RecursiveListAllFiles(Application.dataPath + "/" + Settings.dialogues_path).Where(entry => {
                return entry.IndexOf(".meta") == -1 && entry.IndexOf(".json") > 0;
            }).ToList();
        }

        public void SyncNaming()
        {
            var entries = GetAllEntries();

            //int changes = 0;
            Dictionary<string, Action> changes = new Dictionary<string, Action>();

            entries.ForEach(entry => {
                if (entry.IndexOf(".json") == -1)
                    return;

                string contents = File.ReadAllText(entry);

                DialogueTreeEntity tree = JsonUtility.FromJson<DialogueTreeEntity>(contents);

                string name = entry.Replace("\\", "/").Replace(Application.dataPath + "/" + Settings.dialogues_path + "/", "").Replace(".json", "");

                if (name != tree.name) {
                    // save back
                    changes.Add(tree.name + " => " + name, () => {
                        string nm = tree.name;
                        tree.name = name;

                        File.WriteAllText(entry, JsonUtility.ToJson(tree));
                        Debug.Log("Dialogue tree [" + nm + "] has been renamed to [" + name + "]");
                    });
                }
            });

            if (changes.Count > 0) {
                string text = "There're total of " + changes.Count + " name changes can be synchronized. Do you want to synchronize these tree names?\n";

                foreach (var item in changes) {
                    text += "\n" + item.Key;
                }

                if (EditorUtility.DisplayDialog("", text, "Yes", "No")) {
                    foreach (var item in changes) {
                        item.Value();
                    }
                }


            } else {
                Debug.Log("No changes detected.");
            }
        }

        public void SavePreferences()
        {
            var contents = JsonUtility.ToJson(Settings);

            string path = Application.dataPath + "/dialoguesmith-prefs.json";

            File.WriteAllText(path, contents);

            Repaint();
        }

        protected abstract void LoadTree(string path);

        protected abstract void InitializeTree();

        protected abstract void InitializeTree(string category);
    }
}
