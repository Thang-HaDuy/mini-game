// FILE: ChunkInteractor.cs (AFTER FIX)
using UnityEngine;

public class ChunkInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask ChunkInteractMask;
    [SerializeField] private LayerMask BoundCheckMask;
    [SerializeField] private Transform PlayerCamera;
    [SerializeField] private float InteractRange = 8f;

    private WorldGenerator worldGen;
    private WorldContext world;

    private void Start()
    {
        worldGen = FindObjectOfType<WorldGenerator>();
        world = worldGen.Context;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray camRay = new Ray(PlayerCamera.position, PlayerCamera.forward);

            if (Physics.Raycast(camRay, out RaycastHit hitInfo, InteractRange, ChunkInteractMask))
            {
                Vector3 targetPoint = hitInfo.point - hitInfo.normal * 0.1f;

                Vector3Int targetBlock = new Vector3Int(
                    Mathf.RoundToInt(targetPoint.x),
                    Mathf.RoundToInt(targetPoint.y),
                    Mathf.RoundToInt(targetPoint.z)
                );

                string chunkName = hitInfo.collider.gameObject.name;

                if (chunkName.Contains("Chunk"))
                {
                    worldGen.SetBlock(targetBlock, 0);
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Ray camRay = new Ray(PlayerCamera.position, PlayerCamera.forward);

            if (Physics.Raycast(camRay, out RaycastHit hitInfo, InteractRange, ChunkInteractMask))
            {
                Vector3 targetPoint = hitInfo.point + hitInfo.normal * 0.1f;

                Vector3Int targetBlock = new Vector3Int(
                    Mathf.RoundToInt(targetPoint.x),
                    Mathf.RoundToInt(targetPoint.y),
                    Mathf.RoundToInt(targetPoint.z)
                );

                if (!Physics.CheckBox(targetBlock, Vector3.one * 0.5f, Quaternion.identity, BoundCheckMask))
                {
                    string chunkName = hitInfo.collider.gameObject.name;

                    if (chunkName.Contains("Chunk"))
                    {
                        worldGen.SetBlock(targetBlock, 2);
                    }
                }
            }
        }
    }
}