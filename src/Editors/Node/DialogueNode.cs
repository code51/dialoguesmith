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
        public override string Title { get { return "Dialogue Node"; } }
        public DialogueEntity entity;
        public Rect startRect;
        public Rect endRect;
        public int textIndex = 0;

        protected Rect textLeft;
        protected Rect textRight;
        protected Dictionary<string, Rect> rects = new Dictionary<string, Rect>() {
            { "text_left", Rect.zero },
            { "text_right", Rect.zero }
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

        public override void DrawUpdate()
        {
            base.DrawUpdate();

            options = new List<KeyValuePair<Rect, OptionEntity>>();

            if (!NodeEditor.Instance.CurrentTree.IsConnected(entity))
                GUI.DrawTexture(startRect = new Rect(4f, 3f, 10f, 10f), FileManager.Instance.LoadTexture("assets/redlight.png"));
            else
                GUI.DrawTexture(startRect = new Rect(4f, 3f, 10f, 10f), FileManager.Instance.LoadTexture("assets/bluelight.png"));

            if (entity.options.Count == 0) {
                if (!NodeEditor.Instance.CurrentTree.IsExtended(entity))
                    GUI.DrawTexture(endRect = new Rect(200f, 3f, 10f, 10f), FileManager.Instance.LoadTexture("assets/greenlight.png"));
                else
                    GUI.DrawTexture(endRect = new Rect(200f, 3f, 10f, 10f), FileManager.Instance.LoadTexture("assets/bluelight.png"));
            }

            //entity.id = base.AddTextInput("Id", entity.id);
            string currentId = entity.id;
            EditorGUILayout.LabelField("Id", EditorStyles.boldLabel);
            entity.id = EditorGUILayout.TextField(entity.id);

            if (currentId != entity.id)
                NodeEditor.Instance.RenameDialogueId(currentId, entity.id);

            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.LabelField("Text" + (entity.texts.Count > 1 ? " #" + (textIndex + 1) : ""), EditorStyles.boldLabel);
            string originalText = entity.texts[textIndex];

            entity.texts[textIndex] = EditorGUILayout.TextArea(entity.texts[textIndex], GUILayout.MinHeight(70f));

            if ((originalText != entity.texts[textIndex] && entity.texts[textIndex].Contains("{")) || 
                (originalText.Contains("{") && !entity.texts[textIndex].Contains("{")))
                NodeEditor.Instance.VariableUpdates();

            if (entity.texts.Count > 1) {
                GUI.DrawTexture(rects["text_left"] = new Rect(60f, 60f, 10f, 10f), FileManager.Instance.LoadTexture("assets/left_arrow.png"));
                GUI.DrawTexture(rects["text_right"] = new Rect(75f, 60f, 10f, 10f), FileManager.Instance.LoadTexture("assets/right_arrow.png"));
            }

            int i = 1;

            float height = 150f;
            float offset = 36f;

            entity.options.ForEach(option => {
                EditorGUILayout.LabelField("Option #" + i);

                Rect rect = new Rect(200f, height, 10f, 10f);
                options.Add(new KeyValuePair<Rect, OptionEntity>(rect, option));

                option.id = entity.id + "." + option.id.Split('.')[1];

                if (!NodeEditor.Instance.CurrentTree.IsOptionExtended(option))
                    GUI.DrawTexture(rect, FileManager.Instance.LoadTexture("assets/greenlight.png"));
                else
                    GUI.DrawTexture(rect, FileManager.Instance.LoadTexture("assets/bluelight.png"));

                string optionOriginalText = option.text ?? "";

                option.text = EditorGUILayout.TextField(option.text);

                option.text = option.text ?? "";

                if ((optionOriginalText != option.text && option.text.Contains("{")) ||
                (optionOriginalText.Contains("{") && !option.text.Contains("{")))
                    NodeEditor.Instance.VariableUpdates();

                height += offset;

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
