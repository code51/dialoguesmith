using DialogueSmith.Entities;
using System;
using System.Collections.Generic;

namespace DialogueSmith.Runtime
{
    public abstract class AbstractRuntimeBuilder<Builder> where Builder : AbstractRuntimeBuilder<Builder>
    {
        protected ListenerRegistry listenerRegistry;
        protected Random random;
        protected Dictionary<string, string> variables;
        protected abstract Builder Instance { get; }

        public AbstractRuntimeBuilder(Random random)
        {
            this.variables = new Dictionary<string, string>();
            this.random = random;
            this.listenerRegistry = new ListenerRegistry();
        }

        public Builder SetVariable(string name, string value)
        {
            this.variables[name] = value;

            return Instance;
        }

        public Builder SetVariables(Dictionary<string, string> variables)
        {
            foreach (var item in variables)
                this.variables[item.Key] = item.Value;

            return Instance;
        }

        public Builder OnPaused(Action<DialogueRuntime> callback)
        {
            listenerRegistry.PausingListeners["on_paused"].Add(callback);

            return Instance;
        }

        public Builder OnUnpaused(Action<DialogueRuntime> callback)
        {
            listenerRegistry.PausingListeners["on_unpaused"].Add(callback);

            return Instance;
        }

        /// <summary>
        /// On every dialogue continued
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Builder OnDialogueContinuing(Action<DialogueRuntime, CurrentDialogue> callback)
        {
            listenerRegistry.DialogueGeneralListeners["on_continued"].Add(callback);

            return Instance;
        }

        /// <summary>
        /// On every dialogue initializing.
        /// Can use this to set up variables etc.
        /// </summary>
        /// <param name="dialogue"></param>
        /// <returns></returns>
        public Builder OnDialogueInitialization(Action<DialogueRuntime, CurrentDialogue> dialogue)
        {
            listenerRegistry.DialogueGeneralListeners["on_initialization"].Add(dialogue);

            return Instance;
        }

        /// <summary>
        /// Called on every ready dialogue.
        /// Can use this for UI building, and preparation
        /// as variables are supposedly be completely applied at this time.
        /// </summary>
        /// <param name="dialogue"></param>
        /// <returns></returns>
        public Builder OnDialogueReady(Action<DialogueRuntime, CurrentDialogue> dialogue)
        {
            listenerRegistry.DialogueGeneralListeners["on_ready"].Add(dialogue);

            return Instance;
        }

        /// <summary>
        /// On every option selected
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Builder OnOptionContinuing(Action<DialogueRuntime, CurrentDialogue, OptionSelection> callback)
        {
            listenerRegistry.GeneralOptionSelectionListeners.Add(callback);

            return Instance;
        }

        /// <summary>
        /// On dialogue tree begin. Use this to set up set up UI, enabling etc.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Builder OnDialogueTreeBegin(Action<DialogueRuntime> callback)
        {
            listenerRegistry.DialogueTreeListeners["on_tree_begin"].Add(callback);

            return Instance;
        }

        /// <summary>
        /// When a dialogue has finished.
        /// Use this to disable UI etc.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Builder OnDialogueTreeFinished(Action<DialogueRuntime> callback)
        {
            listenerRegistry.DialogueTreeListeners["on_tree_finished"].Add(callback);

            return Instance;
        }
    }
}
