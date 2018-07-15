# Dialoguesmith
A simple unity engine dialogue editor

# Installation
clone the project into your /Assets folder.
```
git clone https://github.com/code51/dialoguesmith
```

# Implementations
there're multiple ways to implement the use of your dialogues in the scene. while the major feature of this package is mostly about editing a dialogue tree, other being a convenient way to use them in the scene.

## dialogue manager
A manager to create the builder of the dialogue tree runtime.

#### without entity manager
```c#
using System;
using DialogueSmith.Managers;
using DialogueSmith.Runtime;

...

DialogueManager manager = new DialogueManager(new Random());
```

#### with entity manager
An entity manager helps you locate your dialogues by it's string name, instead of TextAsset. The dialogues have to be placed inside your resources folder.
```
DialogueManager manager = new DialogueManager(new EntityManager(), new Random());
```

## runtime builder
A builder pattern to help building a runtime, including setting up the listener to number of events

```
RuntimeBuilder builder = manager.Build("npm/" + merchantName);
```

### listeners
Most of the listeners have more than 1 overrides that usually accept a known dialogue id.

#### OnDialogueTreeBegin()
called when the runtime has begun. can be used for UI enabling etc.

#### OnDialogueTreeFinished()
called when the runtime/dialogue tree has finished.

#### OnDialogueInitializing()
called when a dialogue is initializing. can be used to apply dialogue specific variables, filters and so on.

#### OnDialogueInitialized()
called when a dialogue has been initialized. Can be used to set up UI for the override that accepts no dialogueId (1 argument only)

#### OnDialogueContinued()
called when a dialogue without selections is continued. and before the next dialogue is initialized.

#### OnOptionSelected()
called when an option has been selected. there're three overrides that's suitable for many use case. 

### example usages
#### registry
```
builder.OnDialogueTreeBegin(EnableDialogueUI)
	.OnDialogueTreeFinished(DisableDialogueUI)
	.OnDialogueInitialized(DialogueUIUpdate)
	.OnOptionSelected(DialogueSelectionUpdate);
```

#### begin runtime
```
DialogueRuntime runtime = builder.build();
```

## Runtime
An object that is being used to maintain the state of the dialogue tree in game. originally created by a DialogueBuilder as explained above.

### Next()
used to continue to the next dialogue. an exception will be thrown if there's an option available for the dialogue. 
the dialogue tree ends when there's no more dialogue available (or not in anyway connected to this).

### SelectOption()
used to select an option. same behaviour when there's no more dialogue available for this option.