using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

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
        {
            // Hide background sprite once card image is set
            cardImageBackground.enabled = false;
        }
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
        {
            cardImage.transform.localScale = Vector3.one;
        }
    }
    void OnMouseEnter()
    {
        if (!Interactions.Instance.PlayerCanHover()) return;
        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, -2, 0);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    void OnMouseExit()
    {
        if (!Interactions.Instance.PlayerCanHover()) return;
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

    void OnMouseDown()
    {
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
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (Card.ManualTargetEffect != null) return;
        transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
    }

    void OnMouseUp()
    {
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (Card.ManualTargetEffect != null)
        {
            EnemyView target = ManualTargetSystem.Instance.EndTargeting(MouseUtil.GetMousePositionInWorldSpace(-1));
            if (target != null && CostSystem.Instance.HasEnoughCost(Card.Cost))
            {
                PlayCardGA playCardGA = new(Card, target);
                ActionSystem.Instance.Perform(playCardGA);
            }
        }
        else
        {
            if (CostSystem.Instance.HasEnoughCost(Card.Cost) && Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, 10f, dropLayer))
            {
                PlayCardGA playCardGA = new(Card);
                ActionSystem.Instance.Perform(playCardGA);
            }
            else
            {
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;
            }
            Interactions.Instance.PlayerIsDragging = false;
        }
    }
}