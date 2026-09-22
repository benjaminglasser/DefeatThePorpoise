using System.Collections.Generic;
using UnityEngine;

public class CreateAsteroidField : MonoBehaviour
{
    [Header("References")]
    public GameObject asteroidPrefab;
    public Transform player;

    [Header("Chunk Settings")]
    [Tooltip("Physical size of each chunk")]
    public float chunkSize = 100f;

    [Tooltip("Chunks are generated within this radius around the forward load center")]
    public int loadRadius = 4;

    [Tooltip("Chunks are only removed after passing this larger radius")]
    public int unloadRadius = 7;

    [Tooltip("Number of asteroids spawned inside each chunk")]
    public int asteroidsPerChunk = 2;

    [Tooltip("Minimum and maximum asteroid scale")]
    public Vector2 minMaxScale = new Vector2(0.35f, 5f);

    [Header("Forward Preloading")]
    [Tooltip("How many chunk lengths ahead of the player the load center is shifted")]
    public float forwardLoadChunks = 4f;

    [Header("Generation")]
    [Tooltip("Seed so the asteroid field stays consistent")]
    public int worldSeed = 12345;


    private Dictionary<Vector3Int, GameObject> activeChunks =
        new Dictionary<Vector3Int, GameObject>();

    private Vector3Int currentLoadCenterChunk;


    private void Start()
    {
        if (asteroidPrefab == null)
        {
            Debug.LogError("No asteroid prefab assigned.");
            enabled = false;
            return;
        }

        if (player == null)
        {
            Debug.LogError("No player assigned.");
            enabled = false;
            return;
        }

        unloadRadius = Mathf.Max(unloadRadius, loadRadius + 1);

        currentLoadCenterChunk = GetLoadCenterChunk();

        RefreshChunks();
    }


    private void Update()
    {
        Vector3Int newLoadCenterChunk = GetLoadCenterChunk();

        if (newLoadCenterChunk != currentLoadCenterChunk)
        {
            currentLoadCenterChunk = newLoadCenterChunk;
            RefreshChunks();
        }
    }


    private Vector3Int GetLoadCenterChunk()
    {
        Vector3 loadCenterPosition =
            player.position +
            player.forward * (forwardLoadChunks * chunkSize);

        return GetChunkCoordinate(loadCenterPosition);
    }


    private Vector3Int GetChunkCoordinate(Vector3 position)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.y / chunkSize),
            Mathf.FloorToInt(position.z / chunkSize)
        );
    }


    private void RefreshChunks()
    {
        LoadNearbyChunks();
        RemoveDistantChunks();
    }


    private void LoadNearbyChunks()
    {
        for (int x = -loadRadius; x <= loadRadius; x++)
        {
            for (int y = -loadRadius; y <= loadRadius; y++)
            {
                for (int z = -loadRadius; z <= loadRadius; z++)
                {
                    Vector3Int offset =
                        new Vector3Int(x, y, z);

                    if (offset.magnitude > loadRadius)
                        continue;

                    Vector3Int chunkCoordinate =
                        currentLoadCenterChunk + offset;

                    if (!activeChunks.ContainsKey(chunkCoordinate))
                    {
                        CreateChunk(chunkCoordinate);
                    }
                }
            }
        }
    }


    private void RemoveDistantChunks()
    {
        List<Vector3Int> chunksToRemove =
            new List<Vector3Int>();

        Vector3Int playerChunk =
            GetChunkCoordinate(player.position);

        foreach (KeyValuePair<Vector3Int, GameObject> pair in activeChunks)
        {
            Vector3Int offset =
                pair.Key - playerChunk;

            if (offset.magnitude > unloadRadius + forwardLoadChunks)
            {
                Destroy(pair.Value);
                chunksToRemove.Add(pair.Key);
            }
        }

        foreach (Vector3Int coordinate in chunksToRemove)
        {
            activeChunks.Remove(coordinate);
        }
    }


    private void CreateChunk(Vector3Int chunkCoordinate)
    {
        GameObject chunk =
            new GameObject("Asteroid Chunk " + chunkCoordinate);

        chunk.transform.SetParent(transform);

        Vector3 chunkOrigin = new Vector3(
            chunkCoordinate.x * chunkSize,
            chunkCoordinate.y * chunkSize,
            chunkCoordinate.z * chunkSize
        );

        chunk.transform.position = chunkOrigin;

        activeChunks.Add(chunkCoordinate, chunk);

        Random.State previousRandomState = Random.state;

        Random.InitState(GetChunkSeed(chunkCoordinate));

        for (int i = 0; i < asteroidsPerChunk; i++)
        {
            GameObject asteroid =
                Instantiate(
                    asteroidPrefab,
                    chunk.transform
                );

            asteroid.transform.localPosition =
                new Vector3(
                    Random.Range(0f, chunkSize),
                    Random.Range(0f, chunkSize),
                    Random.Range(0f, chunkSize)
                );

            asteroid.transform.localRotation =
                Random.rotation;

            float scale =
                Random.Range(
                    minMaxScale.x,
                    minMaxScale.y
                );

            asteroid.transform.localScale =
                asteroidPrefab.transform.localScale * scale;
        }

        Random.state = previousRandomState;
    }


    private int GetChunkSeed(Vector3Int coordinate)
    {
        unchecked
        {
            int hash = worldSeed;

            hash = hash * 31 + coordinate.x;
            hash = hash * 31 + coordinate.y;
            hash = hash * 31 + coordinate.z;

            return hash;
        }
    }
}