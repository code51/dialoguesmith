using DialogueSmith.Entities;
using UnityEngine;

namespace DialogueSmith
{
    public interface IEntityLoader
    {
        DialogueTreeEntity LoadTree(string name);

        DialogueTreeEntity LoadTree(TextAsset textAsset);
    }
}
