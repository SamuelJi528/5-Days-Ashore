using UnityEngine;

public class TerrainTreeConverter : MonoBehaviour
{
    [Header("Inputs")]
    public Terrain terrain;

    [Header("Options")]
    public bool clearTerrainTreesAfterConvert = true;
    public string parentName = "Converted Trees";

    [ContextMenu("Convert Terrain Trees")]
    public void ConvertTrees()
    {
        if (!terrain)
        {
            Debug.LogWarning("[TerrainTreeConverter] No Terrain assigned.");
            return;
        }

        var data = terrain.terrainData;
        if (data == null)
        {
            Debug.LogWarning("[TerrainTreeConverter] Terrain has no TerrainData.");
            return;
        }

        var instances   = data.treeInstances;
        var prototypes  = data.treePrototypes;

        if (instances == null || instances.Length == 0)
        {
            Debug.Log("[TerrainTreeConverter] No tree instances found on this terrain.");
            return;
        }

        Transform parent = new GameObject(parentName).transform;
        parent.SetParent(transform, false);

        int spawned = 0;

        foreach (var tree in instances)
        {
            int protoIndex = tree.prototypeIndex;
            if (protoIndex < 0 || protoIndex >= prototypes.Length)
                continue;

            GameObject protoPrefab = prototypes[protoIndex].prefab;
            if (!protoPrefab)
                continue;

            Vector3 worldPos = Vector3.Scale(tree.position, data.size) + terrain.transform.position;
            worldPos.y = terrain.SampleHeight(worldPos) + terrain.transform.position.y;

            Quaternion rot = Quaternion.Euler(0f, tree.rotation * Mathf.Rad2Deg, 0f);

            GameObject go = Instantiate(protoPrefab, worldPos, rot, parent);

            Vector3 baseScale = protoPrefab.transform.localScale;
            go.transform.localScale = new Vector3(
                baseScale.x * tree.widthScale,
                baseScale.y * tree.heightScale,
                baseScale.z * tree.widthScale
            );

            spawned++;
        }

        if (clearTerrainTreesAfterConvert)
        {
            data.treeInstances = new TreeInstance[0];
            terrain.Flush();
        }

        Debug.Log($"[TerrainTreeConverter] Converted {spawned} trees into GameObjects{(clearTerrainTreesAfterConvert ? " and cleared terrain trees" : "")}.");
    }
}
