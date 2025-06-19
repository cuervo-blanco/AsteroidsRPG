using UnityEngine;

public class RocketWobble : MonoBehaviour
{
    private Vector3 initialLocalPos;

    public float wobbleAmplitude = 0.05f;
    public float wobbleFrequency = 2f;
    public float shakeIntensity = 0.02f;
    public float shakeSpeed = 15f;

    public Vector2 inputDirection = Vector2.zero;

    void Start() {
        initialLocalPos = transform.localPosition;
    }

    void Update()
    {
        float wobbleOffset = Mathf.Sin(Time.time * wobbleFrequency * 2 * Mathf.PI) * wobbleAmplitude;

        Vector2 shakeOffset = Vector2.zero;
        if (inputDirection != Vector2.zero)
        {
            shakeOffset = Random.insideUnitCircle * shakeIntensity;
        }

        transform.localPosition = initialLocalPos + new Vector3(shakeOffset.x, wobbleOffset + shakeOffset.y, 0f);
    }
}

