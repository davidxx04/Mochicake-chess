using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance { get; private set; }

    public enum PopupEvent
    {
        Castling,
        EnPassant,
        Promotion,
        Check
    }

    [SerializeField] private ParticleSystem captureParticlePrefab;
    [SerializeField] private Image popupImagePrefab;
    [SerializeField] private Transform popupCanvas;
    [SerializeField] private Sprite castlingSprite;
    [SerializeField] private Sprite enPassantSprite;
    [SerializeField] private Sprite promotionSprite;
    [SerializeField] private Sprite checkSprite;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayCaptureParticles(Vector3 position)
    {
        if (captureParticlePrefab == null)
            return;

        ParticleSystem particles = Instantiate(captureParticlePrefab, position, Quaternion.identity);
        Destroy(particles.gameObject, 2f);
    }

    public void ShowPopup(PopupEvent eventType)
    {
        if (popupImagePrefab == null || popupCanvas == null)
            return;

        Sprite sprite = GetPopupSprite(eventType);
        if (sprite == null)
            return;

        Image img = Instantiate(popupImagePrefab, popupCanvas);
        img.sprite = sprite;

        Color color = img.color;
        color.a = 1f;
        img.color = color;

        StartCoroutine(AnimatePopup(img));
    }

    private Sprite GetPopupSprite(PopupEvent eventType)
    {
        switch (eventType)
        {
            case PopupEvent.Castling:
                return castlingSprite;
            case PopupEvent.EnPassant:
                return enPassantSprite;
            case PopupEvent.Promotion:
                return promotionSprite;
            case PopupEvent.Check:
                return checkSprite;
            default:
                return null;
        }
    }

    private IEnumerator AnimatePopup(Image img)
    {
        if (img == null)
            yield break;

        RectTransform rect = img.rectTransform;

        Vector3 punchStart = Vector3.one * 3f;
        Vector3 punchEnd = Vector3.one;
        float punchDuration = 0.1f;

        rect.localScale = punchStart;

        float elapsed = 0f;
        while (elapsed < punchDuration)
        {
            if (img == null)
                yield break;

            float t = elapsed / punchDuration;
            rect.localScale = Vector3.Lerp(punchStart, punchEnd, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.localScale = punchEnd;

        yield return new WaitForSeconds(0.5f);

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * 30f;
        float fadeDuration = 0.5f;
        elapsed = 0f;

        Color startColor = img.color;

        while (elapsed < fadeDuration)
        {
            if (img == null)
                yield break;

            float t = elapsed / fadeDuration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            Color color = startColor;
            color.a = Mathf.Lerp(1f, 0f, t);
            img.color = color;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (img != null)
        {
            rect.anchoredPosition = endPos;
            Color color = img.color;
            color.a = 0f;
            img.color = color;
            Destroy(img.gameObject);
        }
    }
}
