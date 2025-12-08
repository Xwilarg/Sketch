Handle localization in your games easily

## How to use

Call `Translate.Instance.SetLanguages` at the start of the game to set which languages your game will have, these languages must have appropriate JSON files in Assets/Resources/
```cs
Translate.Instance.SetLanguages(new string[]
{
    "english", "french", "japanese"
});
```
For example, this imply having 3 files: english.json, french.json and japanese.json

You can then call `Translate.Instance.Tr` from anywhere in your code
```cs
Translate.Instance.Tr("mainmenu_start");
```
This will look for the `mainmenu_start` key in the current language JSON

To change the current language, call `Translate.Instance.CurrentLanguage`

To translate a TMP_Text, add the `TMP_Translate` component to it so it translate any key in the following format `{{key}}`