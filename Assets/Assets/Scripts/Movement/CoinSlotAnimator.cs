using UnityEngine;
using UnityEngine.UI;

public class CoinSlotAnimator : MonoBehaviour {
    [SerializeField] Animator animator;
    [SerializeField] Image iconImage;

    void Awake() {
        if (!animator) animator = GetComponent<Animator>();
        if (!iconImage) iconImage = GetComponent<Image>();
    }

    public void SetIcon(Sprite s) => iconImage.sprite = s;

    public void PlayDrain() => animator.SetTrigger("Drain");

    public void ResetState() {
        animator.Rebind();
        iconImage.enabled = true;
        gameObject.SetActive(true);
    }

    public void OnDrainFinished() {
        GetComponentInParent<CoinUI>()?.RemoveSlot(this);
        gameObject.SetActive(false);
    }
}

