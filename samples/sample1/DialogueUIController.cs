using System;
using System.Collections.Generic;
using DialogueSmith.Managers;
using DialogueSmith.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUIController : MonoBehaviour
{
    /// <summary>
    /// if you want to manually use inspector
    /// </summary>
    public TextAsset dialogueTreeText;

    /// <summary>
    /// if you want to specify the tree name.
    /// </summary>
    public string dialogueTreeName;

    public Text text;
    public Transform container;
    public Transform sampleOption;

    protected Transform selectionsContainer;
    protected RuntimeBuilder runtimeBuilder;
    protected EntityManager entityManager;
    protected DialogueManager dialogueManager;
    protected DialogueRuntime runtime;

    private void Start()
    {
        selectionsContainer = container.Find("selections");
        dialogueManager = new DialogueManager(new EntityManager("dialogues"));

        if (dialogueTreeText != null) {
            runtimeBuilder = dialogueManager.Build(dialogueTreeText);
        } else if (dialogueTreeName != "") {
            runtimeBuilder = dialogueManager.Build(dialogueTreeName);
        }

        // Dialogue registry
        if (runtimeBuilder != null) {
            runtimeBuilder.OnDialogueTreeBegin(DialogueTreeBegin)
                .OnDialogueTreeFinished(DialogueTreeFinished)
                .OnDialogueInitialized(DialogueUIUpdate)
                .OnOptionSelected(DialogueOptionSelected);
        }
    }

    private void Update()
    {
        if (runtime != null) {
            if (Input.GetKeyDown(KeyCode.X))
                runtime = runtime.Next();
        } else {
            // to begin the dialogue
            if (Input.GetKeyDown(KeyCode.C))
                runtime = runtimeBuilder.Build();
        }
    }

    protected void DialogueOptionSelected(CurrentDialogue dialogue, OptionSelection selection)
    {
        for (int i = 0; i < selectionsContainer.childCount; i++) {
            if (selectionsContainer.GetChild(i).name == "row_selection")
                Destroy(selectionsContainer.GetChild(i).gameObject);
        }
    }

    protected void DialogueTreeBegin()
    {
        container.gameObject.SetActive(true);
    }

    protected void DialogueTreeFinished()
    {
        container.gameObject.SetActive(false);

        selectionsContainer.gameObject.SetActive(false);
    }

    protected void DialogueUIUpdate(CurrentDialogue dialogue)
    {
        text.text = dialogue.Text;

        selectionsContainer.gameObject.SetActive(false);

        if (dialogue.HasSelections) {
            selectionsContainer.gameObject.SetActive(true);

            dialogue.Selections.ForEach(selection => {
                var option = GameObject.Instantiate(sampleOption);

                option.name = "row_selection";

                option.gameObject.SetActive(true);

                option.Find("text").GetComponent<Text>().text = selection.Text;

                option.GetComponent<Button>().onClick.AddListener(() => {
                    runtime.SelectOption(selection);
                });

                option.SetParent(selectionsContainer);
            });
        }
    }
}
