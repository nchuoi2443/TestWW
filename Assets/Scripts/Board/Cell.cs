using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int BoardX { get; private set; }

    public int BoardY { get; private set; }

    public Item Item { get; private set; }

    public Cell NeighbourUp { get; set; }

    public Cell NeighbourRight { get; set; }

    public Cell NeighbourBottom { get; set; }

    public Cell NeighbourLeft { get; set; }

    public bool CellStatus { get; set; }

    public NormalItem NormalItemInCell { get; set; }

    public bool IsEmpty => NormalItemInCell == null;

    private readonly Vector3 _iniPosition;
    public Vector3 IniPosition => _iniPosition;

    public Cell()
    {
        _iniPosition = new Vector3();
    }

    public void Setup(int cellX, int cellY)
    {
        this.BoardX = cellX;
        this.BoardY = cellY;
        _iniPosition.Set(transform.position.x, transform.position.y, transform.position.z);
    }

    public bool IsNeighbour(Cell other)
    {
        return BoardX == other.BoardX && Mathf.Abs(BoardY - other.BoardY) == 1 ||
            BoardY == other.BoardY && Mathf.Abs(BoardX - other.BoardX) == 1;
    }


    public void Free()
    {
        Item = null;
    }

    public void Assign(Item item)
    {
        Item = item;
        Item.SetCell(this);
    }

    public void ApplyItemPosition(bool withAppearAnimation)
    {
        Item.SetViewPosition(this.transform.position);

        if (withAppearAnimation)
        {
            Item.ShowAppearAnimation();
        }
    }

    internal void Clear()
    {
        if (Item != null)
        {
            Item.Clear();
            Item = null;
        }
    }

    internal bool IsSameType(Cell other)
    {
        return Item != null && other.Item != null && Item.IsSameType(other.Item);
    }

    //this use for explode one item when it get 3 cells
    internal void ExplodeItem()
    {
        if (Item == null) return;

        Item.ExplodeView();
        Item = null;
    }

    internal void AnimateItemForHint()
    {
        Item.AnimateForHint();
    }

    internal void StopHintAnimation()
    {
        Item.StopAnimateForHint();
    }

    internal void ApplyItemMoveToPosition()
    {
        Item.AnimationMoveToPosition();
    }
}
