using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int BoardX { get; private set; }
    public int BoardY { get; private set; }
    public int BoardZ { get; private set; }

    public bool IsClickable { get; private set; }

    public Item Item { get; private set; }
    public bool IsEmpty => Item == null;

    public void Setup(int cellX, int cellY, int cellZ)
    {
        this.BoardX = cellX;
        this.BoardY = cellY;
        this.BoardZ = cellZ;
    }
    public void Assign(Item item)
    {
        Item = item;
        Item.SetCell(this);
    }
    public void Free()
    {
        Item = null;
        SetHighlight(false);
    }
    public void SetHighlight(bool isExposed)
    {
        IsClickable = isExposed;

        if (Item != null && Item.View != null)
        {
            SpriteRenderer sr = Item.View.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = isExposed ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);
            }
        }
    }

    public void ApplyItemPosition(bool withAppearAnimation)
    {
        Item.SetViewPosition(this.transform.position);
        if (withAppearAnimation) Item.ShowAppearAnimation();
    }

    internal void Clear()
    {
        if (Item != null)
        {
            Item.Clear();
            Item = null;
        }
    }

    internal void ExplodeItem()
    {
        if (Item == null) return;
        Item.ExplodeView();
        Item = null;
    }

    internal bool IsSameType(Cell other)
    {
        return Item != null && other.Item != null && Item.IsSameType(other.Item);
    }
}