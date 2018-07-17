using System;
using DialogueSmith.Entities;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace DialogueSmith.Editors.Node
{
    [Serializable]
    public abstract class BaseNode
    {
        public Rect Window;
        public int Id { get; set; }
        public bool IsParentToAttachedNode;
        public BaseNode ParentNode;
        public abstract string Title { get; }
        protected Dictionary<Rect, Action> actions = new Dictionary<Rect, Action>();

        //public string Title { get { return Entity == null ? "" : Entity.window.title; } }
        //public abstract BaseEntity Entity { get; }

        public virtual void DrawUpdate(NodeEditor editor)
        {
            this.actions = new Dictionary<Rect, Action>();
        }

        public virtual void DrawUpdate(BaseEditor editor)
        {
            this.actions = new Dictionary<Rect, Action>();
        }

        protected void AddAction(Rect rect, string texture, Action action)
        {
            this.actions.Add(rect, action);

            GUI.DrawTexture(rect, TextureLoader.Instance.LoadTexture(texture));
        }

        public void ClickedUpdate(Vector2 mousePosition)
        {
            foreach (var item in actions) {
                Rect rect = item.Key;
                rect.x += Window.x;
                rect.y += Window.y;

                if (rect.Contains(mousePosition))
                    item.Value();
            }
        }

        public Vector2 GetStartPoint()
        {
            return new Vector2(Window.x, Window.y + 7f);
        }

        public Vector2 GetEndPoint()
        {
            return new Vector2(Window.x + Window.width - 8f, Window.y + 7f);
        }

        public virtual string TextInput(string label, string value, string description)
        {
            EditorGUILayout.LabelField(new GUIContent(label, description), EditorStyles.boldLabel);
            if (description != null) {
                //EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
            }
            return EditorGUILayout.TextField(value);
        }

        public virtual string TextInputLine(string label, string value)
        {
            Vector2 dimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            EditorGUIUtility.labelWidth = dimensions.x;
            return EditorGUILayout.TextField(label, value);
        }

        public virtual string OptionInputLine(string label, string value)
        {
            Vector2 dimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            EditorGUIUtility.labelWidth = dimensions.x;
            return EditorGUILayout.TextField(label, value, GUILayout.Width(120f));
        }

        public virtual bool ToggleInputLine(string label, bool value)
        {
            Vector2 dimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            EditorGUIUtility.labelWidth = dimensions.x;
            return EditorGUILayout.Toggle(label, value);
        }

        public virtual int IntegerInputLine(string label, int value)
        {
            Vector2 dimensions = GUI.skin.label.CalcSize(new GUIContent(label));
            EditorGUIUtility.labelWidth = dimensions.x;
            return EditorGUILayout.IntField(label, value);
        }
    }
}
