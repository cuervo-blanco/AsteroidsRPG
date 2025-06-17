using UnityEngine;

public class Boundaries : MonoBehaviour {
    private Camera mainCam;
    private Vector2 screenBounds;

    public float objectWidth = 0.5f;
    public float objectHeight = 0.5f;


    void Start()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        objectWidth = sr.bounds.extents.x;
        objectHeight = sr.bounds.extents.y;

        mainCam = Camera.main;
        float camHeight = mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;
        screenBounds = new Vector2(camWidth, camHeight);
    }

    void Update()
    {

    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, -screenBounds.x + objectWidth, screenBounds.x - objectWidth);
        pos.y = Mathf.Clamp(pos.y, -screenBounds.y + objectHeight, screenBounds.y - objectHeight);

        transform.position = pos;
    }


}
