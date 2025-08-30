using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using DG.Tweening;
using System;

public class CardView : MonoBehaviour
{
    [SerializeField] private TMP_Text Name;
    [SerializeField] private TMP_Text Description;
    [SerializeField] private TMP_Text Cost;
    [SerializeField] private SpriteRenderer cardImage;             // The card art sprite to scale
    [SerializeField] private SpriteRenderer cardImageBackground;   // Fixed size background sprite
    [SerializeField] private GameObject wrapper;
    [SerializeField] private LayerMask dropLayer;

    public Card Card { get; private set; }
    public Vector3 dragStartPosition;
    public Quaternion dragStartRotation;

    // -------- Selection mode (for choose/discard, etc.) --------
    private bool selectionEnabled = false;
    private Action<CardView> selectionCallback;

    private void OnDisable() { transform.KillTweensRecursive(); }
    private void OnDestroy() { transform.KillTweensRecursive(); }

    public void SetSelectionEnabled(bool enabled, Action<CardView> onClick)
    {
        selectionEnabled = enabled;
        selectionCallback = enabled ? onClick : null;

        // Kill all tweens on this card & children before changing visuals
        transform.KillTweensRecursive();

        // During selection, don't animate scale (hover preview provides clarity)
        if (!enabled)
        {
            if (gameObject.activeInHierarchy) transform.DOScale(1f, 0.12f);
            else transform.localScale = Vector3.one;
        }
        // If you want a tiny cue when entering selection, you could tint/outline instead of scaling.
    }

    /// <summary>Small feedback when selected by click (suppressed during selection to avoid tween overlap).</summary>
    public void PulseSelected()
    {
        if (selectionEnabled) return; // avoid extra tweens in selection mode
        transform.KillTweensRecursive();
        if (!gameObject.activeInHierarchy) return;

        var s = DOTween.Sequence();
        s.Append(transform.DOScale(1.12f, 0.08f));
        s.Append(transform.DOScale(1.05f, 0.08f));
    }

    // -----------------------------------------------------------

    public void Setup(Card card)
    {
        Card = card;
        Name.text = card.Name;
        Description.text = card.Description;
        Cost.text = card.Cost.ToString();

        SetCardImage(card.Image);
    }

    private void SetCardImage(Sprite newSprite)
    {
        cardImage.sprite = newSprite;
        FitCardImageToBackground();

        if (cardImageBackground != null)
            cardImageBackground.enabled = false; // hide placeholder bg after setting art
    }

    private void FitCardImageToBackground()
    {
        if (cardImage.sprite == null)
        {
            Debug.LogWarning("No sprite assigned to cardImage.");
            return;
        }

        if (cardImageBackground == null)
        {
            Debug.LogWarning("cardImageBackground is not assigned.");
            return;
        }

        // Get background size in world units
        Vector2 bgSize = cardImageBackground.bounds.size;

        // Get sprite size in world units
        float spriteWidth = cardImage.sprite.rect.width / cardImage.sprite.pixelsPerUnit;
        float spriteHeight = cardImage.sprite.rect.height / cardImage.sprite.pixelsPerUnit;

        // Calculate scale factor to fit inside background while preserving aspect ratio
        float scaleX = bgSize.x / spriteWidth;
        float scaleY = bgSize.y / spriteHeight;
        float scaleFactor = Mathf.Min(scaleX, scaleY);
        scaleFactor = Mathf.Max(scaleFactor, 1f);

        // Apply scale uniformly
        cardImage.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
    }

    public void ResetCardImageScale()
    {
        if (cardImage != null)
            cardImage.transform.localScale = Vector3.one;
    }

    void OnMouseEnter()
    {
        if (previewMode) return;  
        if (!Interactions.Instance.PlayerCanHover()) return;

        // Keep wrapper visible during selection so layout doesn't jump
        if (!selectionEnabled) wrapper.SetActive(false);

        Vector3 pos = new(transform.position.x, -2, 0);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    void OnMouseExit()
    {
        if (previewMode) return;  
        if (!Interactions.Instance.PlayerCanHover()) return;

        CardViewHoverSystem.Instance.Hide();

        if (!selectionEnabled) wrapper.SetActive(true);
    }

    void OnMouseDown()
    {
        if (previewMode) return;  
        // Selection mode takes priority over normal interactions
        if (selectionEnabled)
        {
            // When choosing card, immediately hide hover so it doesn't linger
            CardViewHoverSystem.Instance.Hide();
            selectionCallback?.Invoke(this);
            return;
        }

        if (!Interactions.Instance.PlayerCanInteract()) return;

        if (Card.ManualTargetEffect != null)
        {
            ManualTargetSystem.Instance.StartTargeting(transform.position);
        }
        else
        {
            Interactions.Instance.PlayerIsDragging = true;
            wrapper.SetActive(true);
            CardViewHoverSystem.Instance.Hide();
            dragStartPosition = transform.position;
            dragStartRotation = transform.rotation;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
        }
    }

    void OnMouseDrag()
    {
        if (previewMode) return;  
        if (selectionEnabled) return; // no dragging during selection
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (Card.ManualTargetEffect != null) return;

        transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
    }

    void OnMouseUp()
    {
        if (previewMode) return;  
        if (selectionEnabled) return; // click already handled in OnMouseDown
        if (!Interactions.Instance.PlayerCanInteract()) return;

        if (Card.ManualTargetEffect != null)
        {
            EnemyView target = ManualTargetSystem.Instance.EndTargeting(MouseUtil.GetMousePositionInWorldSpace(-1));
            if (target != null && CostSystem.Instance.HasEnoughCost(Card.Cost))
            {
                ActionSystem.Instance.Perform(new PlayCardGA(Card, target));
            }
        }
        else
        {
            if (CostSystem.Instance.HasEnoughCost(Card.Cost) &&
                Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, 10f, dropLayer))
            {
                ActionSystem.Instance.Perform(new PlayCardGA(Card));
            }
            else
            {
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;
            }
            Interactions.Instance.PlayerIsDragging = false;
        }
    }
    // Add near the other fields
    private bool previewMode = false;

    // Add this helper
    public void SetPreview(bool value)
    {
        previewMode = value;

        // Ensure nothing tries to animate/hover/drag
        transform.KillTweensRecursive();

        // If your Card prefab has a Collider/Collider2D, disable it for previews
        var col2D = GetComponent<Collider2D>();
        if (col2D) col2D.enabled = !value;
        var col3D = GetComponent<Collider>();
        if (col3D) col3D.enabled = !value;

        // Keep wrapper visible in previews (so layout looks correct)
        if (wrapper) wrapper.SetActive(true);
    }

    // Optional convenience so you can feed CardData directly
    public void SetupFromData(CardData data)
    {
        if (data == null) return;
        var model = new Card(data);    // your runtime Card wrapper
        Setup(model);
    }
    
}