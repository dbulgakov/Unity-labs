using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Texture")]
    [SerializeField] private Texture2D levelTexture;

    [Header("Tiles Prefabs")]
    [SerializeField] private GameObject prefabWallTile;
    [SerializeField] private GameObject prefabRoadTile;

    [Header("Ball and Road Paint Color")]
    public Color paintColor;

    [HideInInspector] public List<GameObject> roadTilesList = new List<GameObject>();
    [HideInInspector] public GameObject defaultBallRoadTile;

    private Color colorWall = Color.white;
    private Color colorRoad = Color.black;

    private float unitPerPixel;

    private void Awake()
    {
        Generate();
        // Assign the first road tile as the default position for the ball:
        if (roadTilesList.Count > 0)
        {
            defaultBallRoadTile = roadTilesList[0];
        }
        else
        {
            Debug.LogWarning("No road tiles found. Ensure the texture contains road pixels.");
        }
    }

    private void Generate()
    {
        // Ensure the texture is readable
        if (!levelTexture.isReadable)
        {
            Debug.LogError("Level texture must have 'Read/Write Enabled' checked in its import settings.");
            return;
        }

        unitPerPixel = prefabWallTile.transform.lossyScale.x;
        float halfUnitPerPixel = unitPerPixel / 2f;

        float width = levelTexture.width;
        float height = levelTexture.height;

        Vector3 offset = (new Vector3(width / 2f, 0f, height / 2f) * unitPerPixel)
                       - new Vector3(halfUnitPerPixel, 0f, halfUnitPerPixel);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Get pixel color:
                Color pixelColor = levelTexture.GetPixel(x, y);

                Vector3 spawnPos = ((new Vector3(x, 0f, y) * unitPerPixel) - offset);

                if (pixelColor == colorWall)
                {
                    Spawn(prefabWallTile, spawnPos);
                }
                else if (pixelColor == colorRoad)
                {
                    Spawn(prefabRoadTile, spawnPos);
                }
            }
        }
    }

    private void Spawn(GameObject prefabTile, Vector3 position)
    {
        // Fix Y position:
        position.y = prefabTile.transform.position.y;

        // Instantiate and assign parent:
        GameObject obj = Instantiate(prefabTile, position, Quaternion.identity, transform);

        if (prefabTile == prefabRoadTile)
        {
            roadTilesList.Add(obj);
        }
    }
}
