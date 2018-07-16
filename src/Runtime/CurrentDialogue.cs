using System;
using System.Collections.Generic;
using System.Linq;
using DialogueSmith.Entities;
using UnityEngine;

namespace DialogueSmith.Runtime
{
    public class CurrentDialogue
    {
        public DialogueEntity Origin => dialogue;
        public string Id => dialogue.id;
        //public string Text { get { return text; } }
        public string Text
        {
            get {
                return texts[currentTextIndex];
            }
        }

        public Dictionary<string, string> Variables { get { return new Dictionary<string, string>(variables); } }
        public List<OptionSelection> Selections { get { return selections; } }
        public string Actor => dialogue.actor != null && dialogue.actor != "" ? dialogue.actor : (tree.main_actor != null && tree.main_actor != "" ? tree.main_actor : null);

        /// <summary>
        /// Whether this dialogue is extended
        /// </summary>
        public bool IsExtended { get { return tree.IsExtended(dialogue); } }

        /// <summary>
        /// Dialogue is nearing the end
        /// </summary>
        public bool IsEnding => !IsExtended && !HasSelections && currentTextIndex == texts.Count - 1;

        /// <summary>
        /// Whether this dialogue has available options
        /// </summary>
        public bool HasSelections { get { return currentTextIndex == texts.Count - 1 &&  selections.Count > 0; } }

        protected int currentTextIndex = 0;
        protected DialogueTreeEntity tree;
        protected Dictionary<OptionEntity, string> options = new Dictionary<OptionEntity, string>();
        protected DialogueEntity dialogue;
        //protected string text;
        protected List<OptionSelection> selections = new List<OptionSelection>();
        protected Dictionary<string, string> variables = new Dictionary<string, string>();
        //protected List<Predicate<OptionEntity>> optionFilters = new List<Predicate<OptionEntity>>();
        protected List<Func<OptionSelection, OptionSelection>> optionFilters = new List<Func<OptionSelection, OptionSelection>>();
        protected List<Func<string, string>> textFilters = new List<Func<string, string>>();
        protected string originalText;
        protected List<string> texts;
        protected List<string> originalTexts;

        public CurrentDialogue(DialogueTreeEntity tree, DialogueEntity dialogue, List<string> selectedTexts, Dictionary<string, string> variables)
        {
            this.currentTextIndex = 0;
            this.tree = tree;
            this.dialogue = dialogue;
            //this.originalText = this.text = selectedText;
            this.originalTexts = this.texts = new List<string>(selectedTexts);
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

        public bool TextIndexIncrement()
        {
            if (currentTextIndex == texts.Count - 1)
                return false;

            currentTextIndex++;

            return true;
        }

        public CurrentDialogue FilterText(Func<string, string> filter)
        {
            var i = 0;
            foreach (var text in texts.ToList()) {
                texts[i] = filter(text);
                i++;
            }

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
            foreach (var key in variables.Keys.ToList()) {
                this.variables[key] = variables[key];
            }

            //text = originalText;
            texts = new List<string>(originalTexts);


            // apply
            foreach (var item in this.variables) {
                var i = 0;
                foreach (var text in texts.ToList()) {
                    texts[i] = text.Replace("{" + item.Key + "}", item.Value);
                    i++;
                }

                selections.ForEach(selection => {
                    selection.Text = selection.Text.Replace("{" + item.Key + "}", item.Value);
                });
            }

            // reapply filters.
            textFilters.ForEach(filter => {
                var i = 0;
                foreach (var text in texts.ToList()) {
                    texts[i] = filter(text);
                    i++;
                }
            });

            return this;
        }
    }
}
