using UnityEngine;

namespace Sketch.Fishing
{
    [CreateAssetMenu(menuName = "ScriptableObject/FishInfo", fileName = "FishInfo")]
    public class FishInfo : ScriptableObject
    {
        public string Name;
        public string FullName;
        public float MinSize, MaxSize;

        public float Speed;
    }
}