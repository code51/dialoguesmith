using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DialogueSmith.Entities;
using DialogueSmith.Editors.Node;
using DialogueSmith.Helper;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace DialogueSmith.Editors
{
    [Serializable]
    public class NodeEditor : BaseEditor
    {
        public static NodeEditor Instance { get { return instance; } }
        public bool IsScoping = false;
        public DialogueTreeEntity CurrentTree;
        public string OriginalName = "";
        public DialogueTreeNode dialogueTreeNode;

        protected List<DialogueNode> originalNodes = new List<DialogueNode>();
        protected List<DialogueNode> nodes = new List<DialogueNode>();
        protected bool treeInitialized = false;
        protected Vector2 scrollPos;

        protected float viewWidth = 1024f;
        protected float viewHeight = 768f;
        protected static NodeEditor instance;

        protected KeyValuePair<DialogueNode, OptionEntity> activeConnection = new KeyValuePair<DialogueNode, OptionEntity>();

        [MenuItem("Window/Dialoguesmith")]
        public static void ShowEditor()
        {
            instance = EditorWindow.GetWindow<NodeEditor>();
        }
        
        protected void OnGUI()
        {
            Event e = Event.current;

            mousePos = Event.current.mousePosition;

            if (instance == null)
                instance = this;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, true);

            if (!treeInitialized) {
                DialogueManager(e);
            } else {
                Connection(e);
                MouseClick(e);
                ContextMenu(e);
                DrawNodes();
                DrawCurves();
            }
            EditorGUILayout.EndScrollView();
        }

        public void VariableUpdates()
        {
            Dictionary<string, string> originals = CurrentTree.variables.data;
            CurrentTree.variables = new VariableDictionary();

            foreach (var node in nodes) {
                node.entity.texts.ForEach(text => {
                    StringHelper.CurlyExtracts(text).ForEach(variable => {
                        variable = variable.Replace("{", "").Replace("}", "");

                        CurrentTree.variables.data[variable] = originals.ContainsKey(variable) ? originals[variable] : "";
                    });
                });

                node.entity.options.ForEach(option => {
                    StringHelper.CurlyExtracts(option.text).ForEach(variable => {
                        variable = variable.Replace("{", "").Replace("}", "");

                        CurrentTree.variables.data[variable] = originals.ContainsKey(variable) ? originals[variable] : "";
                    });
                });
            }

            dialogueTreeNode.ResetSize();
        }

        protected void MouseClick(Event e)
        {
            if (!(e.type == EventType.MouseDown && e.button == 0))
                return;

            GUI.FocusControl(null);

            foreach (var node in nodes) {
                if (node.IsClicked("text_left", mousePos)) {
                    node.textIndex = node.textIndex == 0 ? node.entity.texts.Count - 1 : node.textIndex - 1;
                }

                if (node.IsClicked("text_right", mousePos)) {
                    node.textIndex = node.textIndex == (node.entity.texts.Count - 1) ? 0 : node.textIndex + 1;
                }

                if (node.IsClicked("add_alt_text", mousePos)) {
                    node.entity.texts.Add("");
                    node.textIndex = node.entity.texts.Count - 1;
                }

                if (node.IsClicked("remove_alt_text", mousePos)) {
                    node.entity.texts.Remove(node.entity.texts[node.textIndex]);
                    node.textIndex = Mathf.Clamp(node.textIndex - 1, 0, node.textIndex);
                }

                if (node.IsClicked("text_continuity_toggle", mousePos)) {
                    node.entity.textContinuity = !node.entity.textContinuity;
                }
            }
        }

        protected void Connection(Event e)
        {
            if (!(e.type == EventType.MouseDown && e.button == 0))
                return;

            if (!IsConnecting()) {
                foreach (var node in nodes) {
                    if (!node.Window.Contains(mousePos))
                        continue;

                    var kvp = node.GetNodeOption(mousePos);

                    if (kvp.Value != null) {
                        CreateActiveConnection(node, kvp);
                    } else {
                        Rect endRect = node.endRect;
                        endRect.y += node.Window.y;
                        endRect.x += node.Window.x;

                        if (endRect.Contains(mousePos)) {
                            CreateActiveConnection(node);
                        }
                    }

                    return;
                }
            } else {
                foreach (var node in nodes) {
                    if (!node.Window.Contains(mousePos))
                        continue;

                    if (activeConnection.Key.entity.id == node.entity.id) {
                        if (node.AbsoluteEndRect.Contains(mousePos)) {
                            ExtendDialogue(node);
                            BreakConnection();
                            return;
                        } else {
                            return;
                        }
                    }

                    if (activeConnection.Value == null) {
                        CurrentTree.RelateDialogue(activeConnection.Key.entity.id, node.entity.id);
                    } else {
                        CurrentTree.RelateOption(activeConnection.Value.id, node.entity.id);
                    }

                    BreakConnection();
                    return;
                }

                // create dialogue
                CreateDialogue(activeConnection);
                BreakConnection();
            }
        }

        protected void Unitialize()
        {
            CurrentTree = null;
            treeInitialized = false;
            dialogueTreeNode = null;
            Debug.Log("WHAT");
            nodes = new List<DialogueNode>();
            originalNodes = new List<DialogueNode>();
        }

        protected void ContextMenu(Event e)
        {
            if (e.type == EventType.ContextClick) {
                if (IsConnecting()) {
                    BreakConnection();
                } else {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Editor/New Tree..."), false, delegate {
                        int result = EditorUtility.DisplayDialogComplex("", "Save this tree first?", "Ok", "Cancel", "No");

                        if (result != 1) {
                            if (result == 0)
                                if (!SaveTree())
                                    return;

                            string[] segments = CurrentTree.name != null ? CurrentTree.name.Split('/') : new string[0];
                            Unitialize();

                            string prefix = "";

                            if (segments.Length > 1) {
                                prefix = String.Join("/", segments.Take(segments.Length - 1).ToArray()) + "/";
                            }

                            InitializeTree(prefix);
                            Repaint();
                        }
                    });

                    BuildLoadMenu("Editor/", menu, () => {
                        int result = EditorUtility.DisplayDialogComplex("", "Save this tree first?", "Ok", "Cancel", "No");

                        if (result != 1) {
                            if (result == 0) {
                                if (!SaveTree())
                                    return false;
                            }

                            return true;
                        }

                        return false;
                    });

                    menu.AddItem(new GUIContent("Editor/Exit..."), false, delegate {
                        int result = EditorUtility.DisplayDialogComplex("", "Save and Exit?", "Ok", "Cancel", "No");

                        if (result != 1) {
                            if (result == 0)
                                if (!SaveTree())
                                    return;
                        }

                        if (result == 1)
                            return;

                        Unitialize();
                    });
                    
                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("New dialogue"), false, delegate {
                        CreateDialogue();
                    });

                    menu.AddItem(new GUIContent("Save"), false, delegate {
                        SaveTree();
                    });

                    if (IsScoping)
                        menu.AddItem(new GUIContent("Exit Scope"), false, delegate {
                            ExitScope();
                        });

                    menu.ShowAsContext();
                }
            } else if (e.type == EventType.MouseDown && e.button == 1 && nodes.Count != 0) {
                GenericMenu menu = new GenericMenu();

                int i = 0;
                foreach (var node in nodes) {
                    if (!node.Window.Contains(e.mousePosition))
                        continue;

                    Rect startRect = node.startRect;
                    startRect.x += node.Window.x;
                    startRect.y += node.Window.y;

                    if (node.AbsoluteStartRect.Contains(mousePos)) {
                        if (CurrentTree.IsConnected(node.entity)) {
                            menu.AddItem(new GUIContent("Disconnect"), false, delegate {
                                CurrentTree.ClearInboundConnection(node.entity.id);
                            });
                        }
                    } else if (node.AbsoluteEndRect.Contains(mousePos)) {
                        if (CurrentTree.IsExtended(node.entity)) {
                            menu.AddItem(new GUIContent("Disconnect"), false, delegate {
                                CurrentTree.ClearOutboundConnection(node.entity.id);
                            });
                        }
                    } else {
                        var option = node.GetNodeOption(e.mousePosition);

                        if (option.Value != null) {
                            if (!CurrentTree.IsOptionExtended(option.Value)) {
                                menu.AddItem(new GUIContent("Remove option"), false, delegate {
                                    RemoveOption(node, option.Value);
                                    node.ResetSize();
                                });
                            } else {
                                menu.AddItem(new GUIContent("Disconnect"), false, delegate {
                                    ClearOptionConnection(option.Value);
                                });
                            }
                        
                        } else {

                            //if (node.entity.texts.Count > 1) {
                            //    menu.AddItem(new GUIContent("Use alt. texts for continuity"), false, delegate {
                            //        node.entity.texts.Remove(node.entity.texts[node.textIndex]);
                            //        node.textIndex = Mathf.Clamp(node.textIndex - 1, 0, node.textIndex);
                            //    });
                            //}

                            if (!CurrentTree.IsExtended(node.entity)) {
                                menu.AddItem(new GUIContent("Add option"), false, delegate {
                                    node.entity.options.Add(new OptionEntity() {
                                        id = node.entity.id + "." + UnityEngine.Random.Range(1, 100).ToString()
                                    });
                                });
                            }

                            if (nodes.Count > 1 && nodes.ElementAt(0) != node) {
                                menu.AddItem(new GUIContent("Remove dialogue"), false, delegate {
                                    RemoveDialogue(node);
                                });

                                //menu.AddItem(new GUIContent("Scope To This"), false, delegate {
                                //    ScopeDialogue(node);
                                //});
                            }
                        }
                    }

                    menu.ShowAsContext();
                    i++;
                    break;
                }
            }
        }

        protected void ClearOptionConnection(OptionEntity option)
        {
            CurrentTree.ClearOptionConnection(option.id);
        }

        protected void CreateActiveConnection(DialogueNode node)
        {
            activeConnection = new KeyValuePair<DialogueNode, OptionEntity>(node, null);
        }

        protected void CreateActiveConnection(DialogueNode node, KeyValuePair<int, OptionEntity> option)
        {
            activeConnection = new KeyValuePair<DialogueNode, OptionEntity>(node, option.Value);
        }

        protected void ResetTree()
        {
            treeInitialized = false;

            InitializeTree();
        }

        protected void ExitScope()
        {
            nodes = originalNodes;

            IsScoping = false;
        }

        protected void ScopeDialogue(DialogueNode dialogue)
        {
            originalNodes = nodes;

            List<DialogueNode> scopedNodes = new List<DialogueNode>();

            List<DialogueEntity> dialogues = CurrentTree.FindConnectedDialogues(dialogue.entity, nodes);

            dialogues.ForEach(dlg => {
                scopedNodes.Add(FindById(dlg.id));
            });

            nodes = scopedNodes;
            //Windows = scopedWindows;

            IsScoping = true;
        }

        protected void RemoveOption(DialogueNode dialogue, OptionEntity option)
        {
            dialogue.entity.options.Remove(option);
        }

        //protected void CreateOptionResponseDialogue(DialogueNode origin, KeyValuePair<int, OptionEntity> option)
        //{
        //    var dialogue = new DialogueEntity() {
        //        id = UnityEngine.Random.Range(0, 1000000).ToString(),
        //        window = new WindowEntity() {
        //            x = origin.Window.x + 250f,
        //            y = origin.Window.y + (option.Key * 180f),
        //            width = 150f,
        //            height = 100
        //        }
        //    };

        //    nodes.Add(CreateInstance<DialogueNode>().Initialize(dialogue));
        //    CurrentTree.RelateOption(option.Value.id, dialogue.id);
        //}

        protected DialogueNode CreateDialogue()
        {
            var dialogue = new DialogueEntity() {
                id = UnityEngine.Random.Range(0, 1000000).ToString(),
                window = new WindowEntity() {
                    x = mousePos.x - 7f,
                    y = mousePos.y - 7f,
                    width = 150f,
                    height = 100
                }
            };

            //var node = CreateInstance<DialogueNode>().Initialize(dialogue);
            var node = (new DialogueNode()).Initialize(dialogue);

            nodes.Add(node);

            return node;
        }

        protected void CreateDialogue(KeyValuePair<DialogueNode, OptionEntity> activeConnection)
        {
            var node = CreateDialogue();

            if (activeConnection.Value == null)
                CurrentTree.RelateDialogue(activeConnection.Key.entity.id, node.entity.id);
            else
                CurrentTree.RelateOption(activeConnection.Value.id, node.entity.id);
        }

        protected void RemoveDialogue(DialogueNode dialogue)
        {
            List<OptionEntity> options = dialogue.entity.options;

            options.ForEach(option => {
                CurrentTree.ClearOptionConnection(option.id);
            });

            CurrentTree.RemoveRelations(dialogue.entity.id);
            nodes.Remove(dialogue);

            VariableUpdates();
        }

        protected void ExtendDialogue(DialogueNode origin)
        {
            var dialogue = new DialogueEntity() {
                id = UnityEngine.Random.Range(0, 1000000).ToString(),
                window = new WindowEntity() {
                    x = origin.Window.x + 220f,
                    y = origin.Window.y,
                    width = 150f,
                    height = 100
                }
            };

            nodes.Add((new DialogueNode()).Initialize(dialogue));

            CurrentTree.RelateDialogue(origin.entity.id, dialogue.id);
        }

        protected void DrawNodes()
        {

            BeginWindows();

            int i = 0;

            dialogueTreeNode.Window = GUILayout.Window(i, dialogueTreeNode.Window, delegate {
                dialogueTreeNode.DrawUpdate();
                GUI.DragWindow();
            }, dialogueTreeNode.Title);

            i++;


            foreach (var node in nodes) {
                nodes[i-1].Window = GUILayout.Window(i, node.Window, delegate {
                    node.DrawUpdate();
                    GUI.DragWindow();
                }, node.Title);

                i++;
            }

            EndWindows();
        }

        public void RenameDialogueId(string origin, string destination)
        {
            CurrentTree.RenameDialogueId(origin, destination);
        }

        protected void DrawCurves()
        {
            if (!IsScoping)
                DrawNodeCurve(dialogueTreeNode.GetEndPoint(), nodes[0].GetStartPoint());
            else
                DrawNodeCurve(dialogueTreeNode.GetEndPoint(), nodes[0].GetStartPoint(), Color.blue);

            foreach (var item in CurrentTree.dialogue_relations.data) {
                if (FindById(item.Key) == null || FindById(CurrentTree.dialogue_relations.data[item.Key]) == null)
                    continue;

                Vector2 startPoint = FindById(item.Key).GetEndPoint();
                Vector2 endPoint = FindById(CurrentTree.dialogue_relations.data[item.Key]).GetStartPoint();
                

                DrawNodeCurve(startPoint, endPoint);
            }

            foreach (var item in CurrentTree.option_relations.data) {
                string dialogue_id = item.Key.Split('.')[0];

                if (FindById(dialogue_id) == null || FindById(item.Value) == null)
                    continue;
                //if (!Windows.data.ContainsKey(dialogue_id) || !Windows.data.ContainsKey(item.Value))
                //    continue;

                Vector2 startPoint = FindById(dialogue_id).GetOptionStartPosition(item.Key);
                Vector2 endPoint = FindById(item.Value).GetStartPoint();

                DrawNodeCurve(startPoint, endPoint);
            }

            if (IsConnecting()) {
                if (activeConnection.Value == null)
                    DrawNodeCurve(activeConnection.Key.GetEndPoint(), mousePos);
                else
                    DrawNodeCurve(activeConnection.Key.GetOptionStartPosition(activeConnection.Value.id), mousePos);
            }
        }

        protected bool IsConnecting()
        {
            return activeConnection.Key != null;
        }

        protected void BreakConnection()
        {
            activeConnection = new KeyValuePair<DialogueNode, OptionEntity>();
            Repaint();
        }

        private void Update()
        {
            if (IsConnecting()) {
                Repaint();
            }
        }

        protected DialogueNode FindById(string id)
        {
            foreach (var node in nodes)
                if (node.entity.id == id)
                    return node;

            return null;
        }

        protected void DrawNodeCurve(Vector3 startPos, Vector3 endPos, Color color)
        {
            float distance = Mathf.Clamp(Vector3.Distance(startPos, endPos), 0f, 50f);

            Vector3 startTan = startPos + Vector3.right * distance;
            Vector3 endTan = endPos + Vector3.left * distance;

            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 2f);
        }


        protected void DrawNodeCurve(Vector3 startPos, Vector3 endPos)
        {
            DrawNodeCurve(startPos, endPos, Color.black);
        }

        protected override void InitializeTree()
        {
            InitializeTree("");
        }

        protected void InitializeTree(string prefix)
        {
            if (treeInitialized)
                return;

            OriginalName = "";

            CurrentTree = new DialogueTreeEntity();

            if (prefix != "")
                CurrentTree.name = prefix;

            // start dialogue node
            var node = (new DialogueTreeNode()).Initialize(CurrentTree);

            //windows.Add(node);
            dialogueTreeNode = node;

            // initialize first dialogue.
            DialogueEntity dialogue = new DialogueEntity() {
                id = UnityEngine.Random.Range(0, 1000000).ToString(),
                window = new WindowEntity() {
                    x = 250f,
                    y = 10f,
                    width = 150f,
                    height = 100
                }
            };

            nodes.Add((new DialogueNode()).Initialize(dialogue));

            CurrentTree.SetInitialDialogue(dialogue);

            treeInitialized = true;
        }

        protected override void LoadTree(string path)
        {
            //string path = EditorUtility.OpenFilePanel("", EditorPrefs.GetString(SETTING_DIALOGUES_PATH), "json");

            if (path == "")
                return;

            string contents = File.ReadAllText(path);

            CurrentTree = JsonUtility.FromJson<DialogueTreeEntity>(contents);

            nodes = new List<DialogueNode>();

            dialogueTreeNode = (new DialogueTreeNode()).Initialize(CurrentTree);

            CurrentTree.dialogues.ForEach(entity => {
                nodes.Add(new DialogueNode().Initialize(entity));
            });

            OriginalName = CurrentTree.name;

            treeInitialized = true;

            GUI.FocusControl(null);
        }

        protected void OnDestroy()
        {
            
        }

        protected bool SaveTree()
        {
            if (CurrentTree.name == "" || CurrentTree.name == null) {
                EditorUtility.DisplayDialog("", "Dialogue tree name must not be empty.", "Ok");
                return false;
            }

            List<DialogueEntity> dialogues = new List<DialogueEntity>();

            nodes.ForEach(node => {
                node.entity.window.x = node.Window.x;
                node.entity.window.y = node.Window.y;

                dialogues.Add(node.entity);
            });

            CurrentTree.dialogues = dialogues;

            string dialoguePath = EditorPrefs.GetString(SETTING_DIALOGUES_PATH);

            string contents = JsonUtility.ToJson(CurrentTree);

            string path = dialoguePath + "/" + CurrentTree.name + ".json";

            if (OriginalName == "" || OriginalName == null || OriginalName != CurrentTree.name) {
                if (File.Exists(path)) {
                    EditorUtility.DisplayDialog("", "File name " + CurrentTree.name + ".json already exist", "Ok");
                    return false;
                }
            }

            if (OriginalName != "" && OriginalName != null && CurrentTree.name != OriginalName) {
                File.Move(dialoguePath + "/" + OriginalName + ".json", path);
            }

            File.WriteAllText(path, contents);

            OriginalName = CurrentTree.name;

            AssetDatabase.Refresh();

            return true;
        }
    }
}
