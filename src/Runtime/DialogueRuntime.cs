using System;
using System.Collections.Generic;
using DialogueSmith.Entities;
using DialogueSmith.Runtime.Exceptions;

namespace DialogueSmith.Runtime
{
    public class DialogueRuntime
    {
        public bool IsRunning => currentDialogue != null;
        public DialogueTreeEntity Tree => dialogueTree;

        protected DialogueTreeEntity dialogueTree;
        protected Dictionary<string, string> variables;
        protected Random random;
        protected CurrentDialogue currentDialogue;
        protected ListenerRegistry listenerRegistry;

        public DialogueRuntime(DialogueTreeEntity dialogueTree,
            Dictionary<string, string> variables,
            Random random,
            ListenerRegistry listenerRegistry
            )
        {
            this.dialogueTree = dialogueTree;
            this.variables = variables;
            this.random = random;
            this.listenerRegistry = listenerRegistry;
            this.InitializeTree();
        }

        protected void InitializeTree()
        {
            InitializeDialogue(dialogueTree.GetDialogue(dialogueTree.initial_dialogue_id));

            listenerRegistry.DialogueTreeListeners["on_tree_begin"].ForEach(action => {
                action(this);
            });
        }

        protected void InitializeDialogue(DialogueEntity dialogue)
        {
            CurrentDialogue currentDialogue = new CurrentDialogue(dialogueTree, dialogue, dialogue.texts[random.Next(0, dialogue.texts.Count)], variables);

            listenerRegistry.DialogueGeneralListeners["on_initializing"].ForEach(action => {
                action(currentDialogue);
            });

            if (listenerRegistry.DialogueSpecificListeners["on_initializing"].ContainsKey(dialogue.id)) {
                listenerRegistry.DialogueSpecificListeners["on_initializing"][dialogue.id].ForEach(action => {
                    action(currentDialogue);
                });
            }

            this.currentDialogue = currentDialogue;

            listenerRegistry.DialogueGeneralListeners["on_initialized"].ForEach(action => {
                action(currentDialogue);
            });

            if (listenerRegistry.DialogueSpecificListeners["on_initialized"].ContainsKey(dialogue.id)) {
                listenerRegistry.DialogueSpecificListeners["on_initialized"][dialogue.id].ForEach(action => {
                    action(currentDialogue);
                });
            }

            return;
        }

        /// <summary>
        /// Continue the dialogue. Only for an extended type of dialogue.
        /// </summary>
        /// <returns></returns>
        public DialogueRuntime Continue()
        {
            if (currentDialogue.Selections.Count > 0)
                throw new InvalidChoiceException();

            listenerRegistry.DialogueGeneralListeners["on_continued"].ForEach(action => {
                action(currentDialogue);
            });

            if (listenerRegistry.DialogueSpecificListeners["on_continued"].ContainsKey(currentDialogue.Id))
                listenerRegistry.DialogueSpecificListeners["on_continued"][currentDialogue.Id].ForEach(action => {
                    action(currentDialogue);
                });

            if (!dialogueTree.IsExtended(currentDialogue.Origin)) {
                return End();
            }

            InitializeDialogue(dialogueTree.GetDialogue(dialogueTree.dialogue_relations.data[currentDialogue.Id]));

            return this;
        }

        /// <summary>
        /// Continue with a selection
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public DialogueRuntime Continue(OptionSelection selection)
        {
            if (listenerRegistry.DialogueOptionSelectionListeners.ContainsKey(currentDialogue.Id)) {
                listenerRegistry.DialogueOptionSelectionListeners[currentDialogue.Id].ForEach(action => {
                    action(currentDialogue, selection);
                });
            }

            if (listenerRegistry.KnownOptionSelectionListeners.ContainsKey(selection.Option.id)) {
                listenerRegistry.KnownOptionSelectionListeners[selection.Option.id].ForEach(action => {
                    action(currentDialogue, selection);
                });
            }

            listenerRegistry.GeneralOptionSelectionListeners.ForEach(action => {
                action(currentDialogue, selection);
            });

            if (!dialogueTree.option_relations.data.ContainsKey(selection.Option.id)) {
                return End();
            }

            InitializeDialogue(dialogueTree.GetDialogue(dialogueTree.option_relations.data[selection.Option.id]));

            return this;
        }

        protected DialogueRuntime End()
        {
            currentDialogue = null;

            listenerRegistry.DialogueTreeListeners["on_tree_finished"].ForEach(action => {
                action(this);
            });

            return null;
        }
    }
}
