using UnityEngine;

public class CameraFilter : MonoBehaviour
{
    [SerializeField]
    private Color filterColor = Color.cyan;
    private SnapshotFilter _filter;

    private void Awake()
    {
        Shader bloomShader = Shader.Find("Snapshot/Bloom");
        Shader neonShader = Shader.Find("Snapshot/Neon");
        _filter = new NeonFilter(name: "Neon", color: filterColor, shader: bloomShader,
            new BaseFilter(name: "", color: Color.white, shader: neonShader));
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _filter.OnRenderImage(source, destination);
    }
}
