using System;
using System.Collections.Generic;
using DialogueSmith.Entities;
using DialogueSmith.Runtime.Exceptions;

namespace DialogueSmith.Runtime
{
    public class RuntimeBuilder
    {
        protected Dictionary<string, List<Action>> dialogueTreeListeners = new Dictionary<string, List<Action>>() {
            { "on_tree_begin", new List<Action>() },
            { "on_tree_finished", new List<Action>() }
        };

        protected List<Action<CurrentDialogue>> dialogueInitializingListeners = new List<Action<CurrentDialogue>>();
        protected List<Action<CurrentDialogue>> dialogueInitializedListeners = new List<Action<CurrentDialogue>>();
        protected List<Action<CurrentDialogue>> dialogueContinuedListeners = new List<Action<CurrentDialogue>>();
        protected Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>> dialogueOptionSelectionListeners = new Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>>();
        protected Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>> knownOptionSelectionListeners = new Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>>();
        protected List<Action<CurrentDialogue, OptionSelection>> generalOptionSelectionListeners = new List<Action<CurrentDialogue, OptionSelection>>();

        protected Dictionary<string, Dictionary<string, List<Action<CurrentDialogue>>>> dialogueListeners = new Dictionary<string, Dictionary<string, List<Action<CurrentDialogue>>>>() {
            { "on_initializing", new Dictionary<string, List<Action<CurrentDialogue>>>() },
            { "on_initialized", new Dictionary<string, List<Action<CurrentDialogue>>>() },
            { "on_continued", new Dictionary<string, List<Action<CurrentDialogue>>>() }
        };

        protected DialogueTreeEntity dialogueTree;
        protected CurrentDialogue currentDialogue;
        protected Random random;
        protected string currentText;
        protected List<OptionEntity> currentOptions;
        protected Dictionary<string, string> variables;

        public RuntimeBuilder(DialogueTreeEntity dialogueTree, Random random)
        {
            this.dialogueTree = dialogueTree;
            this.variables = new Dictionary<string, string>(dialogueTree.variables.data);
            this.random = random;
        }

        public RuntimeBuilder SetVariable(string name, string value)
        {
            this.variables[name] = value;

            return this;
        }

        /// <summary>
        /// On selected dialogue continued
        /// </summary>
        /// <param name="dialogueId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public RuntimeBuilder OnDialogueContinued(string dialogueId, Action<CurrentDialogue> callback)
        {
            if (!dialogueListeners["on_continued"].ContainsKey(dialogueId))
                dialogueListeners["on_continued"][dialogueId] = new List<Action<CurrentDialogue>>();

            dialogueListeners["on_continued"][dialogueId].Add(callback);

            return this;
        }

        /// <summary>
        /// On every dialogue continued
        /// </summary>
        /// <param name="dialogue"></param>
        /// <returns></returns>
        public RuntimeBuilder OnDialogueContinued(Action<CurrentDialogue> dialogue)
        {
            dialogueContinuedListeners.Add(dialogue);

            return this;
        }

        /// <summary>
        /// On selected dialogue initializing
        /// Can use this to set up variables etc, options validations etc
        /// </summary>
        /// <param name="dialogueId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public RuntimeBuilder OnDialogueInitializing(string dialogueId, Action<CurrentDialogue> callback)
        {
            if (!dialogueListeners["on_initializing"].ContainsKey(dialogueId))
                dialogueListeners["on_initializing"][dialogueId] = new List<Action<CurrentDialogue>>();

            dialogueListeners["on_initializing"][dialogueId].Add(callback);

            return this;
        }

        /// <summary>
        /// On every dialogue initializing.
        /// Can use this to set up variables etc.
        /// </summary>
        /// <param name="dialogue"></param>
        /// <returns></returns>
        public RuntimeBuilder OnDialogueInitializing(Action<CurrentDialogue> dialogue)
        {
            dialogueInitializingListeners.Add(dialogue);

            return this;
        }

        public RuntimeBuilder OnDialogueInitialized(string dialogueId, Action<CurrentDialogue> callback)
        {
            if (!dialogueListeners["on_initialized"].ContainsKey(dialogueId))
                dialogueListeners["on_initialized"][dialogueId] = new List<Action<CurrentDialogue>>();

            dialogueListeners["on_initialized"][dialogueId].Add(callback);

            return this;
        }

        /// <summary>
        /// On every initialized dialogue.
        /// Can use this for UI building, and preparation
        /// as variables are supposedly be completely applied at this time.
        /// </summary>
        /// <param name="dialogue"></param>
        /// <returns></returns>
        public RuntimeBuilder OnDialogueInitialized(Action<CurrentDialogue> dialogue)
        {
            dialogueInitializedListeners.Add(dialogue);

            return this;
        }

        /// <summary>
        /// On known option selected
        /// </summary>
        /// <param name="dialogueId"></param>
        /// <param name="optionId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public RuntimeBuilder OnOptionSelected(string dialogueId, int optionIndex, Action<CurrentDialogue, OptionSelection> callback)
        {
            OptionEntity option = dialogueTree.GetDialogue(dialogueId).options[optionIndex];

            if (!knownOptionSelectionListeners.ContainsKey(option.id))
                knownOptionSelectionListeners[option.id] = new List<Action<CurrentDialogue, OptionSelection>>();

            knownOptionSelectionListeners[option.id].Add(callback);

            return this;
        }

        /// <summary>
        /// On dialogue option selected
        /// </summary>
        /// <param name="dialogueId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public RuntimeBuilder OnOptionSelected(string dialogueId, Action<CurrentDialogue, OptionSelection> callback)
        {
            if (!dialogueOptionSelectionListeners.ContainsKey(dialogueId))
                dialogueOptionSelectionListeners[dialogueId] = new List<Action<CurrentDialogue, OptionSelection>>();

            dialogueOptionSelectionListeners[dialogueId].Add(callback);
            
            return this;
        }

        /// <summary>
        /// On every option selected
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public RuntimeBuilder OnOptionSelected(Action<CurrentDialogue, OptionSelection> callback)
        {
            generalOptionSelectionListeners.Add(callback);

            return this;
        }

        /// <summary>
        /// On dialogue tree begin. Use this to set up set up UI, enabling etc.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public RuntimeBuilder OnDialogueTreeBegin(Action callback)
        {
            dialogueTreeListeners["on_tree_begin"].Add(callback);

            return this;
        }

        /// <summary>
        /// When a dialogue has finished.
        /// Use this to disable UI etc.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public RuntimeBuilder OnDialogueTreeFinished(Action callback)
        {
            dialogueTreeListeners["on_tree_finished"].Add(callback);

            return this;
        }

        public DialogueRuntime Build()
        {
            return new DialogueRuntime(
                dialogueTree,
                variables,
                random,
                dialogueTreeListeners,
                dialogueInitializingListeners,
                dialogueInitializedListeners,
                dialogueContinuedListeners,
                dialogueOptionSelectionListeners,
                knownOptionSelectionListeners,
                generalOptionSelectionListeners,
                dialogueListeners
                );
        }
    }
}
