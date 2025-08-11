using UnityEngine;

public class ScrollBackgroundUI : MonoBehaviour
{
    public float scrollSpeed = 200f;
    private RectTransform[] backgrounds;
    private float floorWidth = 1920f;

    void Start()
    {
        int count = transform.childCount;
        backgrounds = new RectTransform[count];
        for (int i = 0; i < count; i++)
        {
            backgrounds[i] = transform.GetChild(i).GetComponent<RectTransform>();
        }
    }

    void Update()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            Vector2 pos = backgrounds[i].anchoredPosition;
            pos.x -= scrollSpeed * Time.deltaTime;

            if (pos.x <= -floorWidth)
            {
                float rightMostX = backgrounds[0].anchoredPosition.x;
                for (int j = 1; j < backgrounds.Length; j++)
                {
                    if (backgrounds[j].anchoredPosition.x > rightMostX)
                        rightMostX = backgrounds[j].anchoredPosition.x;
                }
                pos.x = rightMostX + floorWidth;
            }

            backgrounds[i].anchoredPosition = pos;
        }
    }
}
