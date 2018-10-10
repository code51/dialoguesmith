using System;
using DialogueSmith.Runtime;
using UnityEngine;
using System.Collections.Generic;

namespace DialogueSmith.Runtime
{
    public class RuntimeFactory : AbstractRuntimeBuilder<RuntimeFactory>
    {
        protected override RuntimeFactory Instance { get { return this; } }
        protected IEntityLoader entityLoader;

        public RuntimeFactory(IEntityLoader entityLoader) : base(new System.Random())
        {
            this.entityLoader = entityLoader;
            this.variables = new Dictionary<string, string>();
        }

        public RuntimeFactory(IEntityLoader entityLoader, System.Random random) : base(random)
        {
            this.entityLoader = entityLoader;
            this.variables = new Dictionary<string, string>();
        }

        public RuntimeFactory() : base(new System.Random())
        {
            this.entityLoader = new EntityLoader();
            this.variables = new Dictionary<string, string>();
        }

        public RuntimeFactory(System.Random random) : base(random)
        {
            this.entityLoader = new EntityLoader();
            this.variables = new Dictionary<string, string>();
        }

        public DialogueRuntime Create(string name)
        {
            return new DialogueRuntime(
                entityLoader.LoadTree(name),
                variables,
                random,
                CloneListenerRegistry()
                );
        }

        public DialogueRuntime Create(TextAsset text)
        {
            return new DialogueRuntime(
                entityLoader.LoadTree(text),
                variables,
                random,
                CloneListenerRegistry()
                );
        }

        /// <summary>
        /// Create a runtime builder, given the TextAsset
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public RuntimeBuilder CreateBuilder(TextAsset text)
        {
            return new RuntimeBuilder(entityLoader.LoadTree(text), random, CloneListenerRegistry());
        }

        protected ListenerRegistry CloneListenerRegistry()
        {
            return new ListenerRegistry() {
                DialogueGeneralListeners = new Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue>>>(listenerRegistry.DialogueGeneralListeners),
                DialogueTreeListeners = new Dictionary<string, List<Action<DialogueRuntime>>>(listenerRegistry.DialogueTreeListeners),
                GeneralOptionSelectionListeners = new List<Action<DialogueRuntime, CurrentDialogue, OptionSelection>>(listenerRegistry.GeneralOptionSelectionListeners),
                PausingListeners = new Dictionary<string, List<Action<DialogueRuntime>>>(listenerRegistry.PausingListeners)
            };
        }

        /// <summary>
        /// Create a runtime builder, given the TextAsset and System.Random
        /// </summary>
        /// <param name="text"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public RuntimeBuilder CreateBuilder(TextAsset text, System.Random random)
        {
            return new RuntimeBuilder(entityLoader.LoadTree(text), random, CloneListenerRegistry());
        }

        /// <summary>
        /// Create a runtime builder, given the name of the dialogue
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public RuntimeBuilder CreateBuilder(string name)
        {
            return new RuntimeBuilder(entityLoader.LoadTree(name), random, CloneListenerRegistry());
        }

        /// <summary>
        /// Create a runtime builder
        /// </summary>
        /// <param name="name"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public RuntimeBuilder CreateBuilder(string name, System.Random random)
        {
            return new RuntimeBuilder(entityLoader.LoadTree(name), random, CloneListenerRegistry());
        }
    }
}
