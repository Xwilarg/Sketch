Visual novel engine working with Ink

Show dialogs workflows within your game and let the text scale properly with its container

## How to use

### Displaying text alone
To display a text character by character, you can attach the `TextDisplay.cs` component to an object containing a `TMP_TextMeshPro`

You can then show it by code
```cs
TextDisplay text; // The component we just added

text.ToDisplay = "Thanks for using Sketch.VisualNovel!"
```
Please note that the text truncate itself to fit the container so make sure there is enough space!

### Using the visual novel engine
To start with, attach the `VNManager.cs` script to an object

Our only required parameter is `Display`, which is an object containing the `TMP_TextMeshPro` and `TextDisplay` components \
This object is the one that will show our text character by character as explained in the section above

From that you can just set your `PlayerInput` to call `OnNextDialogue` and call the engine from script:
```cs
TextAsset compiledStoryFile;
VNManager.Instance.ShowStory(compiledStoryFile);
```

### Ink tags
Some of the [Ink tags](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md#tags) will be interpreted by the engine:
- **#speaker name/none**: Look for the corresponding character and how its name and image, if the characters attribute is empty, will use the parameter given as text
- **#skip true/false**: Toggle skip option manually

### VNManager attributes
All the following attributes are optional
- **Characters**: A list of `VNCharacterInfo`, these allow you to use the speaker tag as mentionned above
- **Container**: Object surrounding your text, is automatically shown or hide depending of the state of the VN
- **Name Panel**: Container surrounding the name text, only shown when a name is shown
- **Name Text**: Show the name of a character, used with the speaker tag as mentionned above
- **Character Image**: Show a character sprite, used with the speaker tag as mentionned above
- **Choice Container**: Gameobject that contain choice prefabs, choices as written as per [Ink documentation](https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md#2-choices)
- **Choice Prefab**: Button that will be spawned inside the choice container (Also need to contain TMP_TextMeshPro as a child of the button)

### Public methods
- **ToggleSkip / OnSkip**: Skip is a feature that will very quickly go through dialogues, it'll automatically stop at the first choice shown
- **ToggleHide / OnHide**: Hide the interface and the character sprite, this is generally used if you want the character to enjoy a background or CG that is shown on screen
- **OnAuto**: Auto is a feature that will slowly go through dialogues so the player don't have to click manually, it'll automatically stop at the first choice shown
- **DisplayNextDialogue**: Manually toggle the next dialogue, if called while the text is being displayed, it'll instead instantly display it