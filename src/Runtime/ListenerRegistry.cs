using DialogueSmith.Runtime;
using System;
using System.Collections.Generic;

namespace DialogueSmith.Runtime
{
    public class ListenerRegistry
    {
        public Dictionary<string, List<Action<DialogueRuntime>>> DialogueTreeListeners = new Dictionary<string, List<Action<DialogueRuntime>>>() {
            { "on_tree_begin", new List<Action<DialogueRuntime>>() },
            { "on_tree_finished", new List<Action<DialogueRuntime>>() }
        };

        public Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue>>> DialogueGeneralListeners = new Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue>>>() {
            { "on_initialization", new List<Action<DialogueRuntime, CurrentDialogue>>() },
            { "on_ready", new List<Action<DialogueRuntime, CurrentDialogue>>() },
            { "on_continued", new List<Action<DialogueRuntime, CurrentDialogue>>() }
        };

        public Dictionary<string, Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue>>>> DialogueSpecificListeners = new Dictionary<string, Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue>>>>() {
            { "on_initialization", new Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue>>>() },
            { "on_ready", new Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue>>>() },
            { "on_continued", new Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue>>>() }
        };

        public List<Action<DialogueRuntime, CurrentDialogue, OptionSelection>> GeneralOptionSelectionListeners = new List<Action<DialogueRuntime, CurrentDialogue, OptionSelection>>();
        public Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue, OptionSelection>>> DialogueOptionSelectionListeners = new Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue, OptionSelection>>>();
        public Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue, OptionSelection>>> KnownOptionSelectionListeners = new Dictionary<string, List<Action<DialogueRuntime, CurrentDialogue, OptionSelection>>>();

        public Dictionary<string, List<Action<DialogueRuntime>>> PausingListeners = new Dictionary<string, List<Action<DialogueRuntime>>>() {
            { "on_paused", new List<Action<DialogueRuntime>>() },
            { "on_unpaused", new List<Action<DialogueRuntime>>() }
        };
    }
}
