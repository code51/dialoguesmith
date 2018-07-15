﻿using System;
using System.Collections.Generic;
using DialogueSmith.Entities;
using DialogueSmith.Runtime.Exceptions;

namespace DialogueSmith.Runtime
{
    public class RuntimeBuilder : AbstractRuntimeBuilder<RuntimeBuilder>
    {
        protected override RuntimeBuilder Instance => this;

        protected DialogueTreeEntity dialogueTree;

        public RuntimeBuilder(DialogueTreeEntity dialogueTree, Random random) : base(random)
        {
            this.dialogueTree = dialogueTree;
            this.variables = new Dictionary<string, string>(dialogueTree.variables.data);
        }

        public RuntimeBuilder(DialogueTreeEntity dialogueTree, Random random, ListenerRegistry listenerRegistry) : base(random)
        {
            this.dialogueTree = dialogueTree;
            this.variables = new Dictionary<string, string>(dialogueTree.variables.data);
            this.listenerRegistry = listenerRegistry;
        }

        /// <summary>
        /// On selected dialogue continued
        /// </summary>
        /// <param name="dialogueId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public RuntimeBuilder OnDialogueContinued(string dialogueId, Action<CurrentDialogue> callback)
        {
            if (!listenerRegistry.DialogueSpecificListeners["on_continued"].ContainsKey(dialogueId))
                listenerRegistry.DialogueSpecificListeners["on_continued"][dialogueId] = new List<Action<CurrentDialogue>>();

            listenerRegistry.DialogueSpecificListeners["on_continued"][dialogueId].Add(callback);

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
            if (!listenerRegistry.DialogueSpecificListeners["on_initializing"].ContainsKey(dialogueId))
                listenerRegistry.DialogueSpecificListeners["on_initializing"][dialogueId] = new List<Action<CurrentDialogue>>();

            listenerRegistry.DialogueSpecificListeners["on_initializing"][dialogueId].Add(callback);

            return this;
        }

        public RuntimeBuilder OnDialogueInitialized(string dialogueId, Action<CurrentDialogue> callback)
        {
            if (!listenerRegistry.DialogueSpecificListeners["on_initialized"].ContainsKey(dialogueId))
                listenerRegistry.DialogueSpecificListeners["on_initialized"][dialogueId] = new List<Action<CurrentDialogue>>();

            listenerRegistry.DialogueSpecificListeners["on_initialized"][dialogueId].Add(callback);

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
            if (!listenerRegistry.DialogueOptionSelectionListeners.ContainsKey(dialogueId))
                listenerRegistry.DialogueOptionSelectionListeners[dialogueId] = new List<Action<CurrentDialogue, OptionSelection>>();

            listenerRegistry.DialogueOptionSelectionListeners[dialogueId].Add(callback);

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

            if (!listenerRegistry.KnownOptionSelectionListeners.ContainsKey(option.id))
                listenerRegistry.KnownOptionSelectionListeners[option.id] = new List<Action<CurrentDialogue, OptionSelection>>();

            listenerRegistry.KnownOptionSelectionListeners[option.id].Add(callback);

            return this;
        }

        public DialogueRuntime Build()
        {
            return new DialogueRuntime(
                dialogueTree,
                variables,
                random,
                listenerRegistry
                );
        }
    }
}
