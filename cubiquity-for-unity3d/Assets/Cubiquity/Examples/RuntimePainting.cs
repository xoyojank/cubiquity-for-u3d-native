using UnityEngine;
using System.Collections;
using Cubiquity;

[RequireComponent(typeof(TerrainVolume))]
public class RuntimePainting : MonoBehaviour
{
    public float brushInnerScaleFactor = 0.5f;
    public float brushOuterRadius = 5.0f;
    public float brushOpacity = 1.0f;

    private TerrainVolume terrainVolume;

    // Use this for initialization
    void Start ()
    {
        terrainVolume = GetComponent<TerrainVolume>();
    }

    // Update is called once per frame
    void Update ()
    {
        if (Camera.current == null)
            return;
        Ray ray = Camera.current.ScreenPointToRay(Input.mousePosition);

        // Perform the raycasting.
        PickSurfaceResult pickResult;
        bool hit = Picking.PickSurface(terrainVolume, ray.origin, ray.direction, 1000.0f, out pickResult);

        if(hit)
        {
            // Use this value to compute the inner radius as a proportion of the outer radius.
            float brushInnerRadius = brushOuterRadius * brushInnerScaleFactor;

            if(Input.GetMouseButton(0))
            {
                float multiplier = 1.0f;
                if(Input.GetKey(KeyCode.LeftShift))
                {
                    multiplier  = -1.0f;
                }
                TerrainVolumeEditor.SculptTerrainVolume(terrainVolume, pickResult.volumeSpacePos.x, pickResult.volumeSpacePos.y, pickResult.volumeSpacePos.z, brushInnerRadius, brushOuterRadius, brushOpacity * multiplier);
            }
        }
    }
}
