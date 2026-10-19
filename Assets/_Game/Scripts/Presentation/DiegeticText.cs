using UnityEngine;

namespace EchoProtocol.Presentation
{
    // Built-in TextMesh uses an overlay shader. World labels must respect walls.
    [RequireComponent(typeof(TextMesh))]
    public sealed class DiegeticText : MonoBehaviour
    {
        [SerializeField] private Shader depthTestedShader;
        private TextMesh label;
        private Material material;

        private void Awake()
        {
            label = GetComponent<TextMesh>();
            if (depthTestedShader == null || label.font == null)
            {
                Debug.LogError("DiegeticText requires a font and its depth-tested shader.", this);
                enabled = false;
                return;
            }
            material = new Material(depthTestedShader);
            GetComponent<Renderer>().sharedMaterial = material;
            RefreshAtlas(label.font);
            Font.textureRebuilt += RefreshAtlas;
        }

        private void RefreshAtlas(Font font)
        {
            if (font == label.font && material != null)
                material.mainTexture = font.material.mainTexture;
        }

        private void OnDestroy()
        {
            Font.textureRebuilt -= RefreshAtlas;
            if (material != null) Destroy(material);
        }
    }
}
