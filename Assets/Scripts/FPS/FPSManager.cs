using Sketch.FPS.Prop;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sketch.FPS
{
    public class FPSManager : MonoBehaviour
    {
        public static FPSManager Instance { private set; get; }

        private readonly List<Switch> _switches = new();

        public bool AreAllSwitchesActive => _switches.All(x => x.IsOn);

        private void Awake()
        {
            Instance = this;
        }

        public void Register(Switch s)
        {
            _switches.Add(s);
        }
    }
}