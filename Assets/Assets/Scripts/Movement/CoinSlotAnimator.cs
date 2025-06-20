using UnityEngine;
using UnityEngine.UI;

public class CoinSlotAnimator : MonoBehaviour {
    private Animator animator;
    private Image iconImage;

    void Awake() {
        animator = GetComponent<Animator>();
        iconImage = GetComponent<Image>();
    }

    public void OnDrainFinished() => Destroy(transform.parent.gameObject);

    public void ResetState() {
        animator.Rebind();
        if (iconImage != null) {
            iconImage.enabled = true;
        }

        if (transform.parent != null) {
            transform.parent.gameObject.SetActive(true);
        }
    }
}

