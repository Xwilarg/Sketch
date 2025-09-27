Handle localization in your games easily

## How to use

```cs
Translate.Instance.SetLanguages(new string[]
{
    "english", "french", "japanese"
});

Translate.Instance.Tr("Your word");
```

Add `TMP_Translate` component to a TMP_Text so it translate sentence in the format `{{key}}`