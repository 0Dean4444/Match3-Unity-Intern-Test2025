using DG.Tweening;
using System;
using UnityEngine;

[Serializable]
public class Item
{
    public Cell Cell { get; private set; }
    public Cell OriginalCell { get; set; }
    public Transform View { get; private set; }

    public virtual void SetView()
    {
        string prefabname = GetPrefabName();
        if (!string.IsNullOrEmpty(prefabname))
        {
            GameObject prefab = Resources.Load<GameObject>(prefabname);
            if (prefab) View = GameObject.Instantiate(prefab).transform;
        }
    }

    protected virtual string GetPrefabName() { return string.Empty; }

    public virtual void SetCell(Cell cell)
    {
        Cell = cell;
    }

    internal void AnimationMoveToPosition()
    {
        if (View == null) return;
        View.DOMove(Cell.transform.position, 0.2f);
    }

    public void SetViewPosition(Vector3 pos)
    {
        if (View) View.position = pos;
    }

    public void SetViewRoot(Transform root)
    {
        if (View) View.SetParent(root);
    }

    internal void ShowAppearAnimation()
    {
        if (View == null) return;
        Vector3 scale = View.localScale;
        View.localScale = Vector3.one * 0.1f;
        View.DOScale(scale, 0.1f);
    }

    internal virtual bool IsSameType(Item other)
    {
        return false;
    }

    internal virtual void ExplodeView()
    {
        if (View)
        {
            View.DOScale(0f, 0.25f).SetEase(Ease.InBack).OnComplete(
                () =>
                {
                    GameObject.Destroy(View.gameObject);
                    View = null;
                }
            );
        }
    }

    internal void Clear()
    {
        Cell = null;
        if (View)
        {
            GameObject.Destroy(View.gameObject);
            View = null;
        }
    }
}