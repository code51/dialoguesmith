using System;
using System.Collections.Generic;
using DialogueSmith.Runtime;
using UnityEngine;
using UnityEngine.UI;
using DialogueSmith;

public class DialogueUIController : MonoBehaviour
{
    public string resourceLoadPath = "dialogues";
    public TextAsset dialogueTreeText;
    public string dialogueTreeName;

    public Text text;
    public Transform container;
    public Transform sampleOption;
    public Button continueButton;

    protected Transform actorContainer;
    protected Transform selectionsContainer;
    protected RuntimeBuilder runtimeBuilder;
    protected RuntimeFactory runtimeFactory;
    protected DialogueRuntime runtime;

    private void Start()
    {
        selectionsContainer = container.Find("selections");
        runtimeFactory = (new RuntimeFactory(new EntityLoader(resourceLoadPath)))
                .OnDialogueTreeBegin(DialogueTreeBegin)
                .OnDialogueTreeFinished(DialogueTreeFinished)
                .OnDialogueReady(DialogueUIUpdate)
                .OnOptionSelected(DialogueOptionSelected)
                .SetVariables(new Dictionary<string, string>() {
                    { "title", "captain" },
                    { "gold", "100f" }
                });

        continueButton.onClick.AddListener(() => runtime.Continue());
        actorContainer = container.Find("actor");
    }

    private void Update()
    {
        if (runtime == null) {
            // begin the dialogue
            if (Input.GetKeyDown(KeyCode.C)) {
                if (dialogueTreeText != null)
                    runtime = runtimeFactory.Create(dialogueTreeText);
                else if (dialogueTreeName != "")
                    runtime = runtimeFactory.Create(dialogueTreeName);
                else
                    container.Find("text").GetComponent<Text>().text = "Please specify the json file, or the name of the dialogue tree.";
            }
        }
    }

    protected void DialogueOptionSelected(CurrentDialogue dialogue, OptionSelection selection)
    {
        for (int i = 0; i < selectionsContainer.childCount; i++) {
            if (selectionsContainer.GetChild(i).name == "row_selection")
                Destroy(selectionsContainer.GetChild(i).gameObject);
        }
    }

    protected void DialogueTreeBegin(DialogueRuntime runtime)
    {
        container.gameObject.SetActive(true);
    }

    protected void DialogueTreeFinished(DialogueRuntime runtime)
    {
        container.gameObject.SetActive(false);
        selectionsContainer.gameObject.SetActive(false);
        this.runtime = null;
    }

    protected void DialogueUIUpdate(CurrentDialogue dialogue)
    {
        text.text = dialogue.Text;

        selectionsContainer.gameObject.SetActive(false);

        continueButton.gameObject.SetActive(!dialogue.HasSelections);

        actorContainer.gameObject.SetActive(dialogue.Actor != null ? true : false);

        if (continueButton.gameObject.activeSelf)
            continueButton.transform.Find("text").GetComponent<Text>().text = dialogue.IsEnding ? "Finish" : "Next";

        if (actorContainer.gameObject.activeSelf)
            actorContainer.Find("text").GetComponent<Text>().text = dialogue.Actor;

        if (dialogue.HasSelections) {
            selectionsContainer.gameObject.SetActive(true);

            dialogue.Selections.ForEach(selection => {
                var option = GameObject.Instantiate(sampleOption);

                option.name = "row_selection";

                option.gameObject.SetActive(true);

                option.Find("text").GetComponent<Text>().text = selection.Text;

                option.GetComponent<Button>().onClick.AddListener(() => runtime.Continue(selection));

                option.SetParent(selectionsContainer);
            });
        }
    }
}
