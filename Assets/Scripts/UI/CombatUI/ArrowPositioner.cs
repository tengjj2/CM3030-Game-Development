using UnityEngine;

public class ArrowPositioner : MonoBehaviour
{
    public LevelData levelData;  // Pull level data
    public float yStep = 1f;     // moving high of each level
    private Vector3 startPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
        UpdatePosition();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        Vector3 newPos = startPos + Vector3.up * yStep * (levelData.level - 1);
        transform.position = newPos;
    }

}