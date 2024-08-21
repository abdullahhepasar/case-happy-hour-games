using UnityEngine;

public class TextureAutoTile : MonoBehaviour
{
    [Header("AUTO TILE TEXTURE")]
    public Material Material;
    public Transform Floor;
    public MeshRenderer mesh;
    public int MaterialIndex;
    public Vector2 AutoTileSizeValue = Vector2.one;

    // Start is called before the first frame update
    void Start()
    {
        AutoTileSize();
    }

    public void AutoTileSize()
    {
        Material material = null;

        if (material == null)
            material = Instantiate(Material) as Material;

        mesh.materials[MaterialIndex] = material;

        float ScaleX = Floor.transform.lossyScale.x;
        float ScaleZ = Floor.transform.lossyScale.z;

        if (Material)
        {
            mesh.materials[MaterialIndex].SetTextureScale("_MainTex",
                new Vector2(ScaleX * AutoTileSizeValue.x, ScaleZ * AutoTileSizeValue.y));
        }
    }
}
