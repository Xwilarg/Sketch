using UnityEngine;

namespace Sketch.VN
{
    [CreateAssetMenu(menuName = "ScriptableObject/VNCharacterInfo", fileName = "VNCharacterInfo")]
    public class VNCharacterInfo : ScriptableObject
    {
        public string Name;
        public string DisplayName;
        public Sprite Image;
        public CharacterOverlayInfo[] Overlays;
    }

    [System.Serializable]
    public class CharacterOverlayInfo
    {
        public string ParentTag;
        public CharacterOverlayContentInfo[] OverlayContent;
    }

    [System.Serializable]
    public class CharacterOverlayContentInfo
    {
        public string Tag;
        public Sprite Image;
    }
}