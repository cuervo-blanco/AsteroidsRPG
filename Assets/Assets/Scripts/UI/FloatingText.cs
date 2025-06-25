using TMPro;
using UnityEngine;
using System.Collections;

public class FloatingText : MonoBehaviour {
    [SerializeField] TMP_Text text;
    [SerializeField] float upwardSpeed = 0.5f;
    [SerializeField] AnimationCurve alphaOverLife =
        AnimationCurve.EaseInOut(0,1,1,0);

    float lifeTimer;
    float lifeTime;

    /// <summary>Initialise and start showing.</summary>
    public void Show(string content, float duration = 1f)
    {
        if (!text) {
            Debug.LogWarning("FloatingText: TMP_Text reference missing.");
            return;
        }

        text.text = content;
        lifeTime  = duration;
        lifeTimer = 0f;
        gameObject.SetActive(true);
    }

    void Update() {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime) {
            Destroy(gameObject);
            return;
        }

        float alpha = alphaOverLife.Evaluate(lifeTimer / lifeTime);
        var c = text.color; c.a = alpha; text.color = c;

        transform.position += Vector3.up * upwardSpeed * Time.deltaTime;
    }

    void LateUpdate() {
        if (Camera.main)
            transform.forward = Camera.main.transform.forward;
    }
}

