using UnityEngine;

public class LoopingBackground : MonoBehaviour
{
    public float scrollSpeed = 1f;
    private float tileHeight;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        tileHeight = sr.bounds.size.y;
    }

    void LateUpdate()
    {
        transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

        if (transform.position.y < -tileHeight)
        {
            float newY = transform.position.y + tileHeight * 2f;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}

