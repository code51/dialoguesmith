using System;
using System.Collections.Generic;
using DialogueSmith.Entities;
using DialogueSmith.Runtime.Exceptions;

namespace DialogueSmith.Runtime
{
    public class DialogueRuntime
    {
        public bool IsRunning { get { return currentDialogue != null; } }

        private DialogueTreeEntity dialogueTree;
        private Dictionary<string, string> variables;
        private Random random;
        private Dictionary<string, List<Action>> dialogueTreeListeners;
        private List<Action<CurrentDialogue>> dialogueInitializingListeners;
        private List<Action<CurrentDialogue>> dialogueInitializedListeners;
        private List<Action<CurrentDialogue>> dialogueContinuedListeners;
        private Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>> dialogueOptionSelectionListeners;
        private Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>> knownOptionSelectionListeners;
        private List<Action<CurrentDialogue, OptionSelection>> generalOptionSelectionListeners;
        private Dictionary<string, Dictionary<string, List<Action<CurrentDialogue>>>> dialogueListeners;
        private CurrentDialogue currentDialogue;

        public DialogueRuntime(DialogueTreeEntity dialogueTree, 
            Dictionary<string, string> variables,
            Random random, 
            Dictionary<string, List<Action>> dialogueTreeListeners,
            List<Action<CurrentDialogue>> dialogueInitializingListeners,
            List<Action<CurrentDialogue>> dialogueInitializedListeners,
            List<Action<CurrentDialogue>> dialogueContinuedListeners,
            Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>> dialogueOptionSelectionListeners,
            Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>> knownOptionSelectionListeners,
            List<Action<CurrentDialogue, OptionSelection>> generalOptionSelectionListeners,
            Dictionary<string, Dictionary<string, List<Action<CurrentDialogue>>>> dialogueListeners)
        {
            this.dialogueTree = dialogueTree;
            this.variables = variables;
            this.random = random;
            this.dialogueTreeListeners = dialogueTreeListeners;
            this.dialogueInitializingListeners = dialogueInitializingListeners;
            this.dialogueInitializedListeners = dialogueInitializedListeners;
            this.dialogueContinuedListeners = dialogueContinuedListeners;
            this.dialogueOptionSelectionListeners = dialogueOptionSelectionListeners;
            this.knownOptionSelectionListeners = knownOptionSelectionListeners;
            this.generalOptionSelectionListeners = generalOptionSelectionListeners;
            this.dialogueListeners = dialogueListeners;
            this.InitializeTree();
        }

        protected void InitializeTree()
        {
            InitializeDialogue(dialogueTree.GetDialogue(dialogueTree.initial_dialogue_id));

            dialogueTreeListeners["on_tree_begin"].ForEach(action => {
                action();
            });
        }

        public DialogueRuntime Next()
        {
            if (currentDialogue.Selections.Count > 0)
                throw new InvalidChoiceException();

            dialogueContinuedListeners.ForEach(action => {
                action(currentDialogue);
            });

            if (dialogueListeners["on_continued"].ContainsKey(currentDialogue.Id))
                dialogueListeners["on_continued"][currentDialogue.Id].ForEach(action => {
                    action(currentDialogue);
                });

            if (!dialogueTree.IsExtended(currentDialogue.Origin)) {
                return End();
            }

            InitializeDialogue(dialogueTree.GetDialogue(dialogueTree.dialogue_relations.data[currentDialogue.Id]));

            return this;
        }

        protected DialogueRuntime End()
        {
            currentDialogue = null;

            dialogueTreeListeners["on_tree_finished"].ForEach(action => {
                action();
            });

            return null;
        }

        protected void InitializeDialogue(DialogueEntity dialogue)
        {
            CurrentDialogue currentDialogue = new CurrentDialogue(dialogueTree, dialogue, dialogue.texts[random.Next(0, dialogue.texts.Count)], variables);

            dialogueInitializingListeners.ForEach(action => {
                action(currentDialogue);
            });

            if (dialogueListeners["on_initializing"].ContainsKey(dialogue.id)) {
                dialogueListeners["on_initializing"][dialogue.id].ForEach(action => {
                    action(currentDialogue);
                });
            }

            this.currentDialogue = currentDialogue;

            dialogueInitializedListeners.ForEach(action => {
                action(currentDialogue);
            });

            if (dialogueListeners["on_initialized"].ContainsKey(dialogue.id)) {
                dialogueListeners["on_initialized"][dialogue.id].ForEach(action => {
                    action(currentDialogue);
                });
            }

            return;
        }

        public DialogueRuntime SelectOption(OptionSelection selection)
        {
            if (dialogueOptionSelectionListeners.ContainsKey(currentDialogue.Id)) {
                dialogueOptionSelectionListeners[currentDialogue.Id].ForEach(action => {
                    action(currentDialogue, selection);
                });
            }

            if (knownOptionSelectionListeners.ContainsKey(selection.Option.id)) {
                knownOptionSelectionListeners[selection.Option.id].ForEach(action => {
                    action(currentDialogue, selection);
                });
            }

            generalOptionSelectionListeners.ForEach(action => {
                action(currentDialogue, selection);
            });

            if (!dialogueTree.option_relations.data.ContainsKey(selection.Option.id)) {
                return End();
            }

            InitializeDialogue(dialogueTree.GetDialogue(dialogueTree.option_relations.data[selection.Option.id]));

            return this;
        }
    }
}
