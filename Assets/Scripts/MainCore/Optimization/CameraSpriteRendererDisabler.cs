using UnityEngine;

namespace MainCore.Optimization
{
    [RequireComponent(typeof(Camera))]
    public class CameraSpriteRendererDisabler : MonoBehaviour
    {
        private SpriteRenderer[] allRenderers;

        private Camera mCamera;
        private Plane[] planes = new Plane[6];

        // Start is called before the first frame update
        void Start()
        {
            mCamera = GetComponent<Camera>();
            GeometryUtility.CalculateFrustumPlanes(mCamera, planes);
        }

        // Update is called once per frame
        void Update()
        {
            allRenderers = FindObjectsOfType<SpriteRenderer>();
            SetOutRenderer();
        }

        void SetOutRenderer()
        {
            for (int i = 0; i < allRenderers.Length; i++)
            {
                var bounds = allRenderers[i].bounds;
                var res = GeometryUtility.TestPlanesAABB(planes, bounds);
                allRenderers[i].enabled = res;
            }
        }
    }
}