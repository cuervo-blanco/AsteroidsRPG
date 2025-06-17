using UnityEngine;

public class AlignBackgrounds : MonoBehaviour
{
    public Transform otherBackground;

    void Start()
    {
        float height = GetComponent<SpriteRenderer>().bounds.size.y;
        otherBackground.position = transform.position + Vector3.up * height;
    }
}

