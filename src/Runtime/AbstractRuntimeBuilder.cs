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

        /// <summary>
        /// On every dialogue continued
        /// </summary>
        /// <param name="dialogue"></param>
        /// <returns></returns>
        public Builder OnDialogueContinued(Action<CurrentDialogue> dialogue)
        {
            listenerRegistry.DialogueGeneralListeners["on_continued"].Add(dialogue);

            return Instance;
        }

        /// <summary>
        /// On every dialogue initializing.
        /// Can use this to set up variables etc.
        /// </summary>
        /// <param name="dialogue"></param>
        /// <returns></returns>
        public Builder OnDialogueInitializing(Action<CurrentDialogue> dialogue)
        {
            listenerRegistry.DialogueGeneralListeners["on_initializing"].Add(dialogue);

            return Instance;
        }

        /// <summary>
        /// On every initialized dialogue.
        /// Can use this for UI building, and preparation
        /// as variables are supposedly be completely applied at this time.
        /// </summary>
        /// <param name="dialogue"></param>
        /// <returns></returns>
        public Builder OnDialogueInitialized(Action<CurrentDialogue> dialogue)
        {
            listenerRegistry.DialogueGeneralListeners["on_initialized"].Add(dialogue);

            return Instance;
        }

        /// <summary>
        /// On every option selected
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Builder OnOptionSelected(Action<CurrentDialogue, OptionSelection> callback)
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
