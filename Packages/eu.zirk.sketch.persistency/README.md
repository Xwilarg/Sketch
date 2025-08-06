Save your data persistently between sessions

## How to use
Create a class that will contains your data, make it inherit `ISaveData`

```cs

public class SaveData : ISaveData
{
    public int PlayerScore { set; get; }
    public List<int> UnlockedAchievements { set; get; } = new();
}
```

Use the `PersistencyManager<T>` instance to interact with it

Modify your data
```cs
PersistencyManager<SaveData>.Instance.SaveData.PlayerScore += 10;
PersistencyManager<SaveData>.Instance.SaveData.UnlockedAchievements.Add(0);
```

Save your data to the disk
```cs
PersistencyManager<SaveData>.Instance.Save();
```

Delete all saved data
```cs
PersistencyManager<SaveData>.Instance.DeleteSave();
```