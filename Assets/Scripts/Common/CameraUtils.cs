using UnityEngine;

namespace Sketch.Common
{
    public static class CameraUtils
    {

        // http://answers.unity.com/answers/502236/view.html
        public static Bounds CalculateBounds(this Camera cam)
        {
            float screenAspect = Screen.width / (float)Screen.height;
            float cameraHeight = cam.orthographicSize * 2;
            Bounds bounds = new(
                cam.transform.position,
                new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
            return bounds;
        }
    }
}
