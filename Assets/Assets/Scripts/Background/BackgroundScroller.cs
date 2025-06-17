using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 1f;

    void Update()
    {
        transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
    }
}

