using System;
using System.Linq;
using DialogueSmith.Entities;
using UnityEditor;
using UnityEngine;

namespace DialogueSmith.Editors.Node
{
    [Serializable]
    public class DialogueTreeNode : BaseNode
    {
        //public override BaseEntity Entity { get { return entity; } }
        public override string Title { get { return "Dialogue Tree"; } }
        public DialogueTreeEntity entity;

        public DialogueTreeNode Initialize(DialogueTreeEntity entity)
        {

            this.entity = entity;
            this.Window = new Rect(10f, 10f, 200f, 50f);
            return this;
        }

        public override void DrawUpdate()
        {

            base.DrawUpdate();
            NodeEditor.Instance.CurrentTree = entity;

            EditorGUILayout.LabelField("Name", EditorStyles.boldLabel);
            entity.name= EditorGUILayout.TextField(entity.name);

            if (entity.variables.Count > 0) {
                EditorGUILayout.LabelField("Variables                       Default", EditorStyles.boldLabel);

                foreach (var key in entity.variables.data.Keys.ToList()) {
                    entity.variables.data[key] = base.AddTextInput(key, entity.variables.data[key]);
                }
            }
        }

        public void ResetSize()
        {
            this.Window.height = 50f;
        }
    }
}
