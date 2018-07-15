# Dialoguesmith
A simple unity engine dialogue editor

# Installation
clone the project into your /Assets folder.
```
git clone https://github.com/code51/dialoguesmith
```

# Implementations
there're multiple ways to implement the use of your dialogues in the scene. while the major feature of this package is mostly about editing a dialogue tree, other being a convenience to use them in the code.

## Runtime factory
A factory derived from AbstractRuntimeBuilder to help creating a runtime, including setting up the listeners to number of events

#### without entity loader
```c#
using System;
using DialogueSmith.Runtime;

...

RuntimeFactory factory = new RuntimeFactory(new Random());
```

#### with entity loader
An entity loader helps you locate your dialogues by it's string name, instead of TextAsset. The dialogues directory have to be placed inside your resources folder.
```c#
RuntimeFactory factory = new RuntimeFactory(new EntityLoader("dialogues"), new Random());
```

### listeners
#### OnDialogueTreeBegin()
called when the runtime has begun. can be used for UI enabling etc.

#### OnDialogueTreeFinished()
called when the runtime/dialogue tree has finished.

#### OnDialogueInitializing()
called when a dialogue is initializing. can be used to apply dialogue specific variables, filters and so on.

#### OnDialogueInitialized()
called when a dialogue has been initialized. Can be used to draw the UI update.

#### OnDialogueContinued()
called when a dialogue without selections is continued. and before the next dialogue is initialized. for selection based continue event, use ```OnOptionSelected()```

#### OnOptionSelected()
called when an option has been selected. 

### example usages
#### registry
```
factory.OnDialogueTreeBegin(HandleDialogueTreeBegin)
	.OnDialogueTreeFinished(HandleDialogueTreeFinished)
	.OnDialogueInitialized(HandleDialogueUpdate)
	.OnOptionSelected(HandleDialogueSelectionUpdate);
```

## Runtime
An object that is being used to maintain the state of the dialogue tree in game. originally created by a ```RuntimeFactory``` as explained above.

#### Continue()
used to proceed the dialogue. there're two overrides available for this method. one without argument, second with the argument that requires OptionSelection object. 
The dialogue tree ends, when there's no longer dialogue available next.

### Usage
create the runtime, with the first dialogue initialization.
```
DialogueRuntime runtime = factory.Create(dialogueTreeText);
```
or 
```
DialogueRuntime runtime = factory.Create(dialogueName);
```

## Runtime Builder
A similar class derived from AbstractRuntimeBuilder to handle more sophisticated cases, introducing more than one overrides for existing methods.

```
RuntimeBuilder builder = factory.CreateBuilder("npc/generic-merchant");
```

### Usage
```
DialogueRuntime runtime = builder.Build();
```