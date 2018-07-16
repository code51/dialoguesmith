using System;
using System.Collections.Generic;
using System.Linq;
using DialogueSmith.Editors.Node;
using UnityEngine;

namespace DialogueSmith.Entities
{
    [Serializable]
    public class DialogueTreeEntity
    {
        public string name;
        public WindowEntity window = new WindowEntity();
        //public DialogueEntity initial_dialogue { get { return dialogue} }
        public string initial_dialogue_id;
        public List<DialogueEntity> dialogues;
        public RelationDictionary dialogue_relations = new RelationDictionary();
        public RelationDictionary option_relations = new RelationDictionary();
        public VariableDictionary variables = new VariableDictionary();
        public List<string> actors = new List<string>();
        public string main_actor;

        public void SetInitialDialogue(DialogueEntity dialogue)
        {
            this.initial_dialogue_id = dialogue.id;
            //initial_dialogue = dialogue;
        }

        public void RelateDialogue(string origin, string destination)
        {
            dialogue_relations.data[origin] = destination;
        }

        public DialogueEntity GetDialogue(string id)
        {
            return dialogues.Find(dialogue => dialogue.id == id);
        }

        /// <summary>
        /// This dialogue has been extended
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool IsExtended(DialogueEntity entity)
        {
            return dialogue_relations.data.ContainsKey(entity.id);
        }

        /// <summary>
        /// This option is extended
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public bool IsOptionExtended(OptionEntity option)
        {
            return option_relations.data.ContainsKey(option.id);
        }

        /// <summary>
        /// this dialogue is connected 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool IsConnected(DialogueEntity entity)
        {
            if (entity.id == initial_dialogue_id)
                return true;

            return dialogue_relations.data.ContainsValue(entity.id) || option_relations.data.ContainsValue(entity.id);
        }

        public void RenameDialogueId(string origin, string destination)
        {
            if (origin == initial_dialogue_id)
                initial_dialogue_id = destination;

            // rename key reference
            if (dialogue_relations.data.ContainsKey(origin)) {
                dialogue_relations.data[destination] = dialogue_relations.data[origin];
                dialogue_relations.data.Remove(origin);
            }

            // rename value reference
            foreach (var dialogueId in dialogue_relations.data.Keys.ToList()) {
                if (dialogue_relations.data[dialogueId] == origin) {
                    dialogue_relations.data[dialogueId] = destination;
                }
            }

            foreach (var optionId in option_relations.data.Keys.ToList()) {
                // rename option value reference
                if (option_relations.data[optionId] == origin) {
                    option_relations.data[optionId] = destination;
                }

                // rename key reference
                if (optionId.IndexOf(origin) == 0) {
                    option_relations.data[destination + "." + optionId.Split('.')[1]] = option_relations.data[optionId];
                    option_relations.data.Remove(optionId);
                }
            }
        }

        public void RemoveRelations(string dialogueId)
        {
            if (dialogue_relations.data.ContainsKey(dialogueId))
                dialogue_relations.data.Remove(dialogueId);

            foreach(var id in dialogue_relations.data.Keys.ToList()) {
                if (dialogue_relations.data[id] == dialogueId)
                    dialogue_relations.data.Remove(id);
            }

            // clear child relation
            foreach(var optionId in option_relations.data.Keys.ToList()) {
                if (option_relations.data[optionId] == dialogueId)
                    option_relations.data.Remove(optionId);
            }
        }

        public void ClearOptionConnection(string optionId)
        {
            option_relations.data.Remove(optionId);
        }

        public void RelateOption(string optionId, string dialogueId)
        {
            option_relations.data[optionId] = dialogueId;
        }

        protected DialogueNode FindById(List<DialogueNode> nodes, string id)
        {
            foreach (var node in nodes)
                if (node.entity.id == id)
                    return node;

            return null;
        }

        public List<DialogueEntity> FindConnectedDialogues(DialogueEntity dialogue, List<DialogueNode> nodes)
        {
            List<DialogueEntity> dialogues = new List<DialogueEntity>();

            dialogues.Add(dialogue);

            foreach (var item in dialogue_relations.data) {
                if (item.Key == dialogue.id) {
                    FindConnectedDialogues(FindById(nodes, item.Value).entity, nodes).ForEach(childDialogue => {
                        dialogues.Add(childDialogue);
                    });
                }
            }

            foreach (var option in dialogue.options) {
                if (IsOptionExtended(option)) {
                    FindConnectedDialogues(FindById(nodes, option_relations.data[option.id]).entity, nodes).ForEach(childDialogue => {
                        dialogues.Add(childDialogue);
                    });
                }
            }

            return dialogues;
        }

        public void ClearOutboundConnection(string id)
        {
            if (dialogue_relations.data.ContainsKey(id))
                dialogue_relations.data.Remove(id);
        }

        public void ClearInboundConnection(string id)
        {
            foreach (var entityId in dialogue_relations.data.Keys.ToList()) {
                if (dialogue_relations.data[entityId] == id)
                    dialogue_relations.data.Remove(entityId);
            }

            foreach (var optionId in option_relations.data.Keys.ToList()) {
                if (option_relations.data[optionId] == id)
                    option_relations.data.Remove(optionId);
            }
        }
    }
}
