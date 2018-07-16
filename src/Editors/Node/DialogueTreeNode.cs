using System;
using System.Linq;
using DialogueSmith.Entities;
using UnityEditor;
using UnityEngine;
using DialogueSmith.Managers;
using System.Collections.Generic;

namespace DialogueSmith.Editors.Node
{
    [Serializable]
    public class DialogueTreeNode : BaseNode
    {
        //public override BaseEntity Entity { get { return entity; } }
        public override string Title { get { return "Dialogue Tree"; } }
        public DialogueTreeEntity entity;

        protected Dictionary<string, Rect> rects = new Dictionary<string, Rect>() {
            { "mark_actor_main", Rect.zero }
        };

        public DialogueTreeNode Initialize(DialogueTreeEntity entity)
        {
            this.entity = entity;
            this.Window = new Rect(10f, 10f, 200f, 50f);
            return this;
        }

        public override void DrawUpdate(NodeEditor editor)
        {
            base.DrawUpdate(editor);


            actions = new Dictionary<Rect, Action>();

            editor.CurrentTree = entity;

            EditorGUILayout.LabelField("Name", EditorStyles.boldLabel);
            entity.name= EditorGUILayout.TextField(entity.name);

            var heightOffset = 77f;
            if (entity.actors.Count > 0) {
                EditorGUILayout.LabelField("Actors", EditorStyles.boldLabel);

                entity.actors.ForEach(actor => {
                    EditorGUILayout.LabelField(actor);
                    string texture = actor == editor.CurrentTree.main_actor ? "circle-filled.png" : "circle.png";
                    AddAction(new Rect(200f, heightOffset, 10f, 10f), texture, () => {
                        if (editor.CurrentTree.main_actor == actor)
                            editor.CurrentTree.main_actor = null;
                        else
                            editor.CurrentTree.main_actor = actor;
                    });

                    heightOffset += 20f;
                });
            }

            if (entity.variables.Count > 0) {
                EditorGUILayout.LabelField("Variables                       Default", EditorStyles.boldLabel);

                foreach (var key in entity.variables.data.Keys.ToList()) {
                    entity.variables.data[key] = base.AddTextInput(key, entity.variables.data[key]);
                }
            }
        }

        public void ResetSize()
        {
            this.Window.height = 60f;
        }
    }
}
