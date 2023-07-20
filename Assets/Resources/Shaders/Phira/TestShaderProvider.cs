using UnityEngine;

public class TestShaderProvider : MonoBehaviour
{
    [SerializeField] private Shader shader;

    [SerializeField] private Material mat;

    private void Start()
    {
        mat = new Material(shader);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, mat);
    }
}