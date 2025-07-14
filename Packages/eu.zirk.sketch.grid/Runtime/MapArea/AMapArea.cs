using UnityEngine;

namespace Sketch.Grid.MapArea
{
    public abstract class AMapArea
    {
        protected AMapArea(Bounds bounds)
        {
            Bounds = bounds;
        }

        public Bounds Bounds { private set; get; }
    }
}