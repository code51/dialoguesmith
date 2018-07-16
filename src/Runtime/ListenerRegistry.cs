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

        public Dictionary<string, List<Action<CurrentDialogue>>> DialogueGeneralListeners = new Dictionary<string, List<Action<CurrentDialogue>>>() {
            { "on_initialization", new List<Action<CurrentDialogue>>() },
            { "on_ready", new List<Action<CurrentDialogue>>() },
            { "on_continued", new List<Action<CurrentDialogue>>() }
        };

        public Dictionary<string, Dictionary<string, List<Action<CurrentDialogue>>>> DialogueSpecificListeners = new Dictionary<string, Dictionary<string, List<Action<CurrentDialogue>>>>() {
            { "on_initialization", new Dictionary<string, List<Action<CurrentDialogue>>>() },
            { "on_ready", new Dictionary<string, List<Action<CurrentDialogue>>>() },
            { "on_continued", new Dictionary<string, List<Action<CurrentDialogue>>>() }
        };

        public List<Action<CurrentDialogue, OptionSelection>> GeneralOptionSelectionListeners = new List<Action<CurrentDialogue, OptionSelection>>();
        public Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>> DialogueOptionSelectionListeners = new Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>>();
        public Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>> KnownOptionSelectionListeners = new Dictionary<string, List<Action<CurrentDialogue, OptionSelection>>>();


    }
}
