using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CardFlipper : MonoBehaviour
{
    [SerializeField] private float flipDuration = 0.5f;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private float revealHoldTime = 0.15f;

    private Image cardImage;
    private RectTransform rectTransform;
    private Coroutine runningFlip;
    private bool isFlipping;

    private void Awake()
    {
        cardImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        if (backSprite != null)
            cardImage.sprite = backSprite;
    }

    public void SetBackSprite(Sprite sprite)
    {
        backSprite = sprite;
        if (cardImage != null && backSprite != null)
            cardImage.sprite = backSprite;
    }

    public void FlipCard(Sprite frontSprite, Action onComplete)
    {
        if (isFlipping)
            return;

        runningFlip = StartCoroutine(FlipRoutine(frontSprite, onComplete));
    }

    private IEnumerator FlipRoutine(Sprite frontSprite, Action onComplete)
    {
        isFlipping = true;

        float duration = Mathf.Max(0.01f, flipDuration);
        float half = duration * 0.5f;
        Vector3 baseScale = rectTransform.localScale;

        // First half: scale X to zero to hide the card.
        float elapsed = 0f;
        while (elapsed < half)
        {
            float t = elapsed / half;
            rectTransform.localScale = new Vector3(Mathf.Lerp(baseScale.x, 0f, t), baseScale.y, baseScale.z);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        rectTransform.localScale = new Vector3(0f, baseScale.y, baseScale.z);

        if (frontSprite != null)
            cardImage.sprite = frontSprite;

        // Second half: scale X back to original size.
        elapsed = 0f;
        while (elapsed < half)
        {
            float t = elapsed / half;
            rectTransform.localScale = new Vector3(Mathf.Lerp(0f, baseScale.x, t), baseScale.y, baseScale.z);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        rectTransform.localScale = baseScale;
        isFlipping = false;
        runningFlip = null;
        if (revealHoldTime > 0f)
            yield return new WaitForSecondsRealtime(revealHoldTime);
        onComplete?.Invoke();
    }
}
