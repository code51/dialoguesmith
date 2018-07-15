using System;
using System.Collections.Generic;
using System.Linq;
using DialogueSmith.Entities;

namespace DialogueSmith.Runtime
{
    public class CurrentDialogue
    {
        public DialogueEntity Origin { get { return dialogue; } }
        public string Id { get { return dialogue.id; } }
        public string Text { get { return text; } }
        public Dictionary<string, string> Variables { get { return new Dictionary<string, string>(variables); } }
        public List<OptionSelection> Selections { get { return selections; } }
        public bool IsExtended { get { return tree.IsExtended(dialogue); } }
        public bool HasSelections { get { return selections.Count > 0; } }

        protected DialogueTreeEntity tree;
        protected Dictionary<OptionEntity, string> options = new Dictionary<OptionEntity, string>();
        protected DialogueEntity dialogue;
        protected string text;
        protected List<OptionSelection> selections = new List<OptionSelection>();
        protected Dictionary<string, string> variables = new Dictionary<string, string>();
        //protected List<Predicate<OptionEntity>> optionFilters = new List<Predicate<OptionEntity>>();
        protected List<Func<OptionSelection, OptionSelection>> optionFilters = new List<Func<OptionSelection, OptionSelection>>();
        protected List<Func<string, string>> textFilters = new List<Func<string, string>>();
        protected string originalText;

        public CurrentDialogue(DialogueTreeEntity tree, DialogueEntity dialogue, string selectedText, Dictionary<string, string> variables)
        {
            this.tree = tree;
            this.dialogue = dialogue;
            this.originalText = this.text = selectedText;
            this.InitializeSelections();
            this.variables = variables;
            this.ApplyVariables(variables);
        }

        protected void InitializeSelections()
        {
            dialogue.options.ForEach(option => {
                selections.Add(new OptionSelection(tree, option, option.text));
            });
        }

        public CurrentDialogue FilterText(Func<string, string> filter)
        {
            text = filter(text);

            return this;
        }

        public CurrentDialogue FilterSelections(Func<OptionSelection, OptionSelection> callback)
        {
            List<OptionSelection> selections = new List<OptionSelection>();

            foreach(var selection in selections.ToList()) {
                var filteredSelection = callback(selection);

                if (filteredSelection == null)
                    continue;

                selections.Add(filteredSelection);
            }

            this.selections = selections;

            return this;
        }

        public CurrentDialogue ApplyVariables(Dictionary<string, string> variables)
        {
            // merge first
            foreach (var item in variables.Keys.ToList()) {
                this.variables[item] = variables[item];
            }

            text = originalText;

            foreach (var item in this.variables) {
                text = text.Replace("{" + item.Key + "}", item.Value);

                selections.ForEach(selection => {
                    selection.Text = selection.Text.Replace("{" + item.Key + "}", item.Value);
                });
            }

            // reapply filters.
            textFilters.ForEach(filter => {
                text = filter(text);
            });

            return this;
        }
    }
}
