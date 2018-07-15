using DialogueSmith.Entities;

namespace DialogueSmith.Runtime
{
    public class OptionSelection
    {
        public string Text;
        public OptionEntity Option { get { return option; } }
        public bool IsExtended { get { return tree.IsOptionExtended(option); } }

        protected OptionEntity option;
        protected DialogueTreeEntity tree;

        public OptionSelection(DialogueTreeEntity tree, OptionEntity option, string text)
        {
            this.tree = tree;
            this.option = option;
            this.Text = text;
        }
    }
}
