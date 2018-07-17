using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DialogueSmith.Entities;
using DialogueSmith.Editor.Node;
using DialogueSmith.Helper;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace DialogueSmith.Editor
{
    [Serializable]
    public class NodeEditor : BaseEditor
    {
        public static NodeEditor Instance { get { return instance; } }
        public bool IsScoping = false;
        public DialogueTreeEntity CurrentTree;
        public string OriginalName = "";
        public DialogueTreeNode dialogueTreeNode;
        public bool ShowIds = false;

        protected List<DialogueNode> originalNodes = new List<DialogueNode>();
        protected List<DialogueNode> nodes = new List<DialogueNode>();
        protected bool treeInitialized = false;
        protected Vector2 scrollPos;
        protected bool needScroll = false;
        protected float viewWidth = 1024f;
        protected float viewHeight = 768f;
        protected static NodeEditor instance;
        protected KeyValuePair<DialogueNode, OptionEntity> activeConnection = new KeyValuePair<DialogueNode, OptionEntity>();
        protected Vector2 lastDragPosition;
        [NonSerialized]
        public bool MissingPath = false;
        protected bool missingPathChecking = false;

        [MenuItem("Window/Dialoguesmith")]
        public static void ShowEditor()
        {
            instance = EditorWindow.GetWindow<NodeEditor>();
        }

        protected void LoadSettings()
        {
            if (Settings == null) {
                string path = Application.dataPath + "/dialoguesmith-prefs.json";

                if (File.Exists(path)) {
                    Settings = JsonUtility.FromJson<EditorSettingEntity>(File.ReadAllText(path));
                } else {
                    Settings = new EditorSettingEntity();
                }
            }
        }

        protected void HandleMissingVendor()
        {
            EditorUtility.DisplayDialog("", "The dialoguesmith folder somehow couldn't be located. Please select the folder. It's necessary to load texture asset etc.", "Ok");

            string path = EditorUtility.OpenFolderPanel("", "Assets", "").Replace("\\", "/");

            if (path == "" || (path != "" && !path.Contains(Application.dataPath))) {
                if (!EditorUtility.DisplayDialog("", "The folder has to be inside the Assets path. Retry?", "Ok", "No")) {
                    MissingPath = false;
                    missingPathChecking = true;
                }
            } else {
                Settings.dialoguesmith_path = path.Replace(Application.dataPath.Replace("\\", "/"), "").Trim('/');
                SavePreferences();
                MissingPath = false;
            }
        }
        
        protected void OnGUI()
        {
            LoadSettings();

            if (MissingPath) {
                HandleMissingVendor();
                return;
            }

            if (missingPathChecking)
                return;

            IncreaseScrollView();

            Event e = Event.current;

            mousePos = Event.current.mousePosition;

            if (instance == null)
                instance = this;

            //GUILayout.BeginArea(new Rect(0, 0, 2500, 2500));

            if (needScroll) {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, true);
                mousePos += scrollPos;
            }

            GUI.backgroundColor = Color.clear;
            GUILayout.Box("", GUILayout.Width(viewWidth), GUILayout.Height(viewHeight));
            GUI.backgroundColor = new Color(1f, 1f, 1f, 1f);

            ScrollMouseDrag(e);

            if (!treeInitialized) {
                DialogueManager(e);
            } else {
                Connection(e);
                MouseClick(e);
                ContextMenu(e);
                DrawNodes();
                DrawCurves();
            }

            if (needScroll)
                EditorGUILayout.EndScrollView();
        }

        protected void ScrollMouseDrag(Event e)
        {
            // middle mouse drag
            if (e.type == EventType.MouseDrag && e.button == 2) {
                if (lastDragPosition != Vector2.zero) {
                    //scrollPos = Vector2.Lerp(scrollPos, scrollPos - ((e.mousePosition - lastDragPosition).normalized) * 10f, 0.3f);
                    scrollPos = scrollPos - ((e.mousePosition - lastDragPosition).normalized) * 10f;
                }

                lastDragPosition = e.mousePosition;
            }
        }

        public void IncreaseScrollView()
        {
            float buffer = 50f;
            float width = 0f;
            float height = 0f;

            nodes.ForEach(node => {
                width = node.Window.x + node.Window.width > width ? node.Window.x + node.Window.width : width;
                height = node.Window.y + node.Window.height > height ? node.Window.y + node.Window.height : height;
            });

            width += buffer;
            height += buffer;

            viewWidth = Mathf.Clamp(width, Screen.width, width);
            viewHeight = Mathf.Clamp(height, Screen.height - 20f, height);

            if (viewWidth > Screen.width || viewHeight > Screen.height)
                needScroll = true;
            else
                needScroll = false;

            if (needScroll)
                Repaint();
        }

        public void ActorsUpdate()
        {
            CurrentTree.actors = new List<string>();

            nodes.ForEach(node => {
                if (node.entity.actor != "" && node.entity.actor != null) {
                    if (!CurrentTree.actors.Contains(node.entity.actor))
                        CurrentTree.actors.Add(node.entity.actor);
                }
            });

            if (CurrentTree.actors.Count == 0)
                CurrentTree.main_actor = null;
            else if (!CurrentTree.actors.Contains(CurrentTree.main_actor))
                CurrentTree.main_actor = null;

            dialogueTreeNode.ResetSize();
        }

        public void VariablesUpdate()
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

            dialogueTreeNode.ClickedUpdate(mousePos);

            foreach (var node in nodes) {
                node.ClickedUpdate(mousePos);
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

        protected void Uninitialize()
        {
            CurrentTree = null;
            treeInitialized = false;
            dialogueTreeNode = null;
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

                    //menu.AddItem(new GUIContent("Editor/New Tree..."), false, delegate {
                    //    int result = EditorUtility.DisplayDialogComplex("", "Save this tree first?", "Ok", "Cancel", "No");

                    //    if (result != 1) {
                    //        if (result == 0)
                    //            if (!SaveTree())
                    //                return;

                    //        string[] segments = CurrentTree.name != null ? CurrentTree.name.Split('/') : new string[0];
                    //        Unitialize();

                    //        string prefix = "";

                    //        if (segments.Length > 1) {
                    //            prefix = String.Join("/", segments.Take(segments.Length - 1).ToArray()) + "/";
                    //        }

                    //        InitializeTree(prefix);
                    //        Repaint();
                    //    }
                    //});

                    BuildLoadMenu("Editor/", menu, () => {
                        int result = EditorUtility.DisplayDialogComplex("", "Save this tree first?", "Ok", "Cancel", "No");

                        if (result != 1) {
                            if (result == 0) {
                                if (!SaveTree())
                                    return false;
                            }

                            Uninitialize();
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

                        Uninitialize();
                        Repaint();
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
                                
                            } else {
                                menu.AddItem(new GUIContent("Disconnect"), false, delegate {
                                    ClearOptionConnection(option.Value);
                                });
                            }
                        
                        } else {

                            menu.AddItem(new GUIContent(node.showId ? "Hide Id" : "Show Id"), false, delegate {
                                node.showId = !node.showId;
                                node.ResetSize();
                            });

                            if (!node.actorAvailability) {
                                if (CurrentTree.actors.Count == 0) {
                                    menu.AddItem(new GUIContent("Set actor"), false, delegate {
                                        node.actorAvailability = true;
                                    });
                                } else {
                                    menu.AddItem(new GUIContent("Set actor/New..."), false, delegate {
                                        node.actorAvailability = true;
                                    });

                                    CurrentTree.actors.ForEach(actor => {
                                        menu.AddItem(new GUIContent("Set actor/" + actor), false, delegate {
                                            node.actorAvailability = true;
                                            node.entity.actor = actor;
                                            ActorsUpdate();
                                        });
                                    });
                                }
                            }

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

            List<DialogueEntity> dialogues = FindConnectedDialogues(dialogue.entity);

            dialogues.ForEach(dlg => {
                scopedNodes.Add(FindById(dlg.id));
            });

            nodes = scopedNodes;
            //Windows = scopedWindows;

            IsScoping = true;
        }

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

            VariablesUpdate();
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
                dialogueTreeNode.DrawUpdate(this);
                GUI.DragWindow();
            }, dialogueTreeNode.Title);

            i++;


            foreach (var node in nodes) {
                nodes[i-1].Window = GUILayout.Window(i, node.Window, delegate {
                    node.DrawUpdate(this);
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

        protected override void InitializeTree(string category)
        {
            if (treeInitialized)
                return;

            OriginalName = "";

            CurrentTree = new DialogueTreeEntity();

            if (category != "")
                CurrentTree.name = category.TrimEnd('/') + "/";

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

            Repaint();

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
                node.entity.window.width = node.Window.width;

                dialogues.Add(node.entity);
            });

            CurrentTree.dialogues = dialogues;

            string dialoguePath = Application.dataPath + "/" + Settings.dialogues_path;

            string contents = JsonUtility.ToJson(CurrentTree);

            string path = dialoguePath + "/" + CurrentTree.name + ".json";

            if (OriginalName == "" || OriginalName == null || OriginalName != CurrentTree.name) {
                if (File.Exists(path)) {
                    EditorUtility.DisplayDialog("", "File name " + CurrentTree.name + ".json already exist", "Ok");
                    return false;
                }
            }

            string dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (OriginalName != "" && OriginalName != null && CurrentTree.name != OriginalName) {
                File.Move(dialoguePath + "/" + OriginalName + ".json.meta", path + ".meta");
                File.Move(dialoguePath + "/" + OriginalName + ".json", path);
            }

            File.WriteAllText(path, contents);

            OriginalName = CurrentTree.name;

            AssetDatabase.Refresh();

            return true;
        }

        protected DialogueNode FindById(List<DialogueNode> nodes, string id)
        {
            foreach (var node in nodes)
                if (node.entity.id == id)
                    return node;

            return null;
        }

        public List<DialogueEntity> FindConnectedDialogues(DialogueEntity dialogue)
        {
            List<DialogueEntity> dialogues = new List<DialogueEntity>();

            dialogues.Add(dialogue);

            foreach (var item in CurrentTree.dialogue_relations.data) {
                if (item.Key == dialogue.id) {
                    FindConnectedDialogues(FindById(nodes, item.Value).entity).ForEach(childDialogue => {
                        dialogues.Add(childDialogue);
                    });
                }
            }

            foreach (var option in dialogue.options) {
                if (CurrentTree.IsOptionExtended(option)) {
                    FindConnectedDialogues(FindById(nodes, CurrentTree.option_relations.data[option.id]).entity).ForEach(childDialogue => {
                        dialogues.Add(childDialogue);
                    });
                }
            }

            return dialogues;
        }
    }
}
