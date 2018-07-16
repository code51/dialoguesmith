using System;
using System.Collections.Generic;
using DialogueSmith.Entities;
using DialogueSmith.Helper;
using DialogueSmith.Managers;
using UnityEditor;
using UnityEngine;

namespace DialogueSmith.Editors.Node
{
    [Serializable]
    public class DialogueNode : BaseNode
    {
        public override string Title => (entity.actor != "" && entity.actor != null) ? 
            entity.actor + "'s" : 
            (NodeEditor.Instance.CurrentTree.main_actor != "" && NodeEditor.Instance.CurrentTree.main_actor != null ? NodeEditor.Instance.CurrentTree.main_actor + "'s" : "Dialogue Node");
        public DialogueEntity entity;
        public Rect startRect;
        public Rect endRect;
        public int textIndex = 0;
        public bool actorAvailability = false;
        protected bool HasActor => (entity.actor != "" && entity.actor != null) || actorAvailability;

        protected Rect textLeft;
        protected Rect textRight;
        protected Dictionary<string, Rect> rects = new Dictionary<string, Rect>() {
            { "text_left", Rect.zero },
            { "text_right", Rect.zero },
            { "remove_alt_text", Rect.zero },
            { "add_alt_text", Rect.zero },
            { "text_continuity_toggle", Rect.zero },
            { "remove_actor", Rect.zero }
        };
        protected Rect originalRect;

        protected List<string[]> variables = new List<string[]>();

        public Rect AbsoluteStartRect
        {
            get {
                Rect startRect = this.startRect;
                startRect.x += Window.x;
                startRect.y += Window.y;

                return startRect;
            }
        }

        public Rect AbsoluteEndRect
        {
            get {
                Rect endRect = this.endRect;
                endRect.x += Window.x;
                endRect.y += Window.y;

                return endRect;
            }
        }
        //public Rect AbsoluteEndRect { get { return new Rect(endRect.x + Window.x, endRect.y + Window.x, Window.width, Window.height); } }

        protected List<KeyValuePair<Rect, OptionEntity>> options = new List<KeyValuePair<Rect, OptionEntity>>();
        public DialogueNode Initialize(DialogueEntity entity)
        {
            this.entity = entity;
            this.Window = originalRect = new Rect(entity.window.x, entity.window.y, entity.window.width, entity.window.height);

            return this;
        }

        public void ResetSize()
        {
            this.Window.height = originalRect.height;
            this.Window.width = originalRect.width;
        }

        public KeyValuePair<int, OptionEntity> GetNodeOption(Vector2 position)
        {
            entity.window.x = Window.x;
            entity.window.y = Window.y;

            Vector2 nodePosition = Vector2.zero;
            KeyValuePair<int, OptionEntity> option = new KeyValuePair<int, OptionEntity>();

            position.x -= Window.x;
            position.y -= Window.y;

            int i = 0;
            options.ForEach(kvp => {
                if (kvp.Key.Contains(position)) {
                    option = new KeyValuePair<int, OptionEntity>(i, kvp.Value);
                }

                i++;
            });

            return option;
            //return nodePosition;
        }

        public Rect GetAbsoluteRect(string name)
        {
            Rect rect = rects[name];

            rect.x += Window.x;
            rect.y += Window.y;

            return rect;
        }

        public bool IsClicked(string name, Vector2 position)
        {
            return GetAbsoluteRect(name).Contains(position);
        }

        public override void DrawUpdate(NodeEditor editor)
        {
            base.DrawUpdate(editor);

            float heightOffset = 3f;

            options = new List<KeyValuePair<Rect, OptionEntity>>();

            if (!editor.CurrentTree.IsConnected(entity))
                GUI.DrawTexture(startRect = new Rect(4f, heightOffset, 10f, 10f), FileManager.Instance.LoadTexture("redlight.png"));
            else
                GUI.DrawTexture(startRect = new Rect(4f, heightOffset, 10f, 10f), FileManager.Instance.LoadTexture("bluelight.png"));

            if (entity.options.Count == 0) {
                if (!editor.CurrentTree.IsExtended(entity))
                    GUI.DrawTexture(endRect = new Rect(200f, heightOffset, 10f, 10f), FileManager.Instance.LoadTexture("greenlight.png"));
                else
                    GUI.DrawTexture(endRect = new Rect(200f, heightOffset, 10f, 10f), FileManager.Instance.LoadTexture("bluelight.png"));
            }

            if (editor.ShowIds) {
                string currentId = entity.id;
                EditorGUILayout.LabelField("Id", EditorStyles.boldLabel);
                entity.id = EditorGUILayout.TextField(entity.id);
                if (currentId != entity.id)
                    editor.RenameDialogueId(currentId, entity.id);

                heightOffset += 36f;
            }

            heightOffset += 21f;

            if ((entity.actor != "" && entity.actor != null) || actorAvailability) {
                EditorGUILayout.LabelField("Actor", EditorStyles.boldLabel);
                string originalActor = entity.actor;
                entity.actor = EditorGUILayout.TextField(entity.actor);
                this.AddAction(new Rect(200f, heightOffset, 10f, 10f), "cross.png", () => {
                    entity.actor = "";
                    actorAvailability = false;
                    ResetSize();
                    editor.ActorsUpdate();
                });
                //GUI.DrawTexture(rects["remove_actor"] = new Rect(45f, heightOffset, 10f, 10f), FileManager.Instance.LoadTexture("cross.png"));
                heightOffset += 36f;

                if (originalActor != entity.actor)
                    editor.ActorsUpdate();
            }

            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.LabelField("Text" + (entity.texts.Count > 1 ? " " + (textIndex + 1) + "/" + entity.texts.Count : ""), EditorStyles.boldLabel);
            string originalText = entity.texts[textIndex];

            entity.texts[textIndex] = EditorGUILayout.TextArea(entity.texts[textIndex], GUILayout.MinHeight(70f));

            if ((originalText != entity.texts[textIndex] && entity.texts[textIndex].Contains("{")) || 
                (originalText.Contains("{") && !entity.texts[textIndex].Contains("{")))
                editor.VariablesUpdate();

            AddAction(new Rect(200f, heightOffset, 10f, 10f), "plus.png", () => {
                entity.texts.Add("");
                textIndex = entity.texts.Count - 1;
            });
            //GUI.DrawTexture(rects["add_alt_text"] = new Rect(200f, heightOffset, 10f, 10f), FileManager.Instance.LoadTexture("plus.png"));

            if (entity.texts.Count > 1) {
                AddAction(new Rect(170f, heightOffset, 10f, 10f), "left_arrow.png", () => {
                    textIndex = textIndex == 0 ? entity.texts.Count - 1 : textIndex - 1;
                });

                AddAction(new Rect(185f, heightOffset, 10f, 10f), "right_arrow.png", () => {
                    textIndex = textIndex == (entity.texts.Count - 1) ? 0 : textIndex + 1;
                });

                AddAction(new Rect(153f, heightOffset -2f, 12f, 12f), entity.textContinuity ? "repeat.png" : "random.png", () => {
                    entity.textContinuity = !entity.textContinuity;
                });

                if (textIndex != 0) {
                    AddAction(new Rect(140f, heightOffset, 10f, 10f), "cross.png", () => {
                        entity.texts.Remove(entity.texts[textIndex]);
                        textIndex = Mathf.Clamp(textIndex - 1, 0, textIndex);
                    });
                    //GUI.DrawTexture(rects["remove_alt_text"] = new Rect(140f, heightOffset, ;10f, 10f), FileManager.Instance.LoadTexture("cross.png"));
                }
            }

            int i = 1;

            float height = heightOffset + 90f;
            float heightIncrement = 36f;

            entity.options.ForEach(option => {
                EditorGUILayout.LabelField("Option #" + i);

                Rect rect = new Rect(200f, height, 10f, 10f);
                options.Add(new KeyValuePair<Rect, OptionEntity>(rect, option));

                option.id = entity.id + "." + option.id.Split('.')[1];

                if (!editor.CurrentTree.IsOptionExtended(option))
                    GUI.DrawTexture(rect, FileManager.Instance.LoadTexture("greenlight.png"));
                else
                    GUI.DrawTexture(rect, FileManager.Instance.LoadTexture("bluelight.png"));

                //if (!editor.CurrentTree.IsOptionExtended(option))
                    AddAction(new Rect(185f, height, 10f, 10f), "cross.png", () => {
                        editor.CurrentTree.ClearOptionConnection(option.id);
                        entity.options.Remove(option);
                        ResetSize();
                    });

                string optionOriginalText = option.text ?? "";

                option.text = EditorGUILayout.TextField(option.text);

                option.text = option.text ?? "";

                if ((optionOriginalText != option.text && option.text.Contains("{")) ||
                (optionOriginalText.Contains("{") && !option.text.Contains("{")))
                    editor.VariablesUpdate();

                height += heightIncrement;

                i++;
            });
        }

        public Vector2 GetOptionStartPosition(string optionId)
        {
            Vector2 pos = Vector2.zero;

            options.ForEach(kvp => {
                if (kvp.Value.id == optionId)
                    pos = new Vector2(Window.x + kvp.Key.center.x, Window.y + kvp.Key.center.y);
            });

            return pos;
        }
    }
}
