using UnityEngine;

public class CloudMove : MonoBehaviour
{
    public float speed = 2f;
    public float destroyX = -15f;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }
}
