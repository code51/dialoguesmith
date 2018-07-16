using System;
using System.Collections.Generic;
using DialogueSmith.Managers;
using DialogueSmith.Runtime;
using UnityEngine;
using UnityEngine.UI;
using DialogueSmith;

public class DialogueUIController : MonoBehaviour
{
    public TextAsset dialogueTreeText;
    public string dialogueTreeName;

    public Text text;
    public Transform container;
    public Transform sampleOption;
    public Button continueButton;

    protected Transform selectionsContainer;
    protected RuntimeBuilder runtimeBuilder;
    protected RuntimeFactory runtimeFactory;
    protected DialogueRuntime runtime;

    private void Start()
    {
        selectionsContainer = container.Find("selections");
        runtimeFactory = (new RuntimeFactory(new EntityLoader("dialogues")))
                .OnDialogueTreeBegin(DialogueTreeBegin)
                .OnDialogueTreeFinished(DialogueTreeFinished)
                .OnDialogueReady(DialogueUIUpdate)
                .OnOptionSelected(DialogueOptionSelected);

        continueButton.onClick.AddListener(() => runtime.Continue());
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

        if (continueButton.gameObject.activeSelf)
            continueButton.transform.Find("text").GetComponent<Text>().text = dialogue.IsEnding ? "Finish" : "Next";

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
