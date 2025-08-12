using UnityEngine;

public class CloudSpawner : MonoBehaviour
{

    public GameObject cloudPrefab;
    public Sprite[] cloudSprites;
    public float spawnInterval = 2f;
    public float spawnX = 10f;
    public Vector2 heightRange = new Vector2(-2f, 2f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating(nameof(SpawnCloud), 0f, spawnInterval);
    }

    void SpawnCloud()
    {
        // Generate cloud under CloudSpawner GameObject
        GameObject newCloud = Instantiate(cloudPrefab, transform);

        // Set position x
        float randomY = Random.Range(heightRange.x, heightRange.y);
        newCloud.transform.position = new Vector3(spawnX, randomY, 0f);

        // Randomly change cloud style
        SpriteRenderer sr = newCloud.GetComponentInChildren<SpriteRenderer>();
        sr.sprite = cloudSprites[Random.Range(0, cloudSprites.Length)];
    }
}
