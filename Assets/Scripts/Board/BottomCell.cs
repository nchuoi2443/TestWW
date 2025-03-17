using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using static UnityEditor.Progress;

public class BottomCell : MonoBehaviour
{
    private int bottomCellX;

    private int bottomCellY;

    private Transform m_root;

    private List<Cell> m_cell;

    private bool[] m_cellStatus;

    private GameManager m_gameManager;

    private List<ListNormalItem> m_listNormalItem;

    private int m_itemsInBoad;
    private class ListNormalItem
    {
        public List<NormalItem> normalItems;

    }
    public BottomCell(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;


        this.bottomCellX = gameSettings.BottomCellX;
        this.bottomCellY = gameSettings.BottomCellY;

        m_cell = new List<Cell>();
        for (int i = 0; i < bottomCellX; i++)
        {
            m_cell.Add(new Cell());
        }

        m_cellStatus = new bool[bottomCellX];
        m_gameManager = FindObjectOfType<GameManager>();
        m_itemsInBoad = gameSettings.BoardSizeX * gameSettings.BoardSizeY;

        CreateBottomCell();
    }

    private void CreateBottomCell()
    {
        Vector3 origin = new Vector3(-bottomCellX * 0.5f + 0.5f, -bottomCellY * 0.5f - 3f, 0f);

        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int x = 0; x < bottomCellX; x++)
        {
            GameObject go = GameObject.Instantiate(prefabBG);
            go.transform.position = origin + new Vector3(x, 0f);
            go.transform.SetParent(m_root);

            Cell cell = go.GetComponent<Cell>();
            cell.Setup(x, bottomCellY - 1);
            m_cellStatus[x] = false;

            m_cell[x] = cell;

        }

    }

    public void AddCell(Cell cell)
    {
        
        for (int i = 0; i < bottomCellX; i++)
        {
            if (m_cell[i].NormalItemInCell == cell.NormalItemInCell && m_gameManager.CurrentGameMode == GameManager.eLevelMode.MOVES)
            {
                return;
            }
        }

        if (m_gameManager.CurrentGameMode == GameManager.eLevelMode.TIMER && CheckBottomContainItem(cell))
        {
            cell.NormalItemInCell.AnimationMoveToPosition();
            GetIndexOfItemInCell(cell);
            return;
        }

        for (int i = 0; i < m_cell.Count; i++)
        {
            if (m_cellStatus[i] == false)
            {
                
                if (cell.NormalItemInCell == null) return;
                cell.NormalItemInCell.View.DOMove(m_cell[i].transform.position, 0.2f).onComplete = () =>
                {
                    FindMatchAndCollapse();
                    if (m_cell[m_cell.Count - 1].NormalItemInCell != null && m_gameManager.CurrentGameMode == GameManager.eLevelMode.MOVES)
                    {
                        m_gameManager.State = GameManager.eStateGame.GAME_OVER;
                    }
                };
                m_cellStatus[i] = true;
                m_cell[i].NormalItemInCell = cell.NormalItemInCell;
                //cell.ApplyItemPosition(true);
                break;
            }
        }
    }

    bool CheckBottomContainItem(Cell cell)
    {
        for (int i = 0; i < m_cell.Count; i++)
        {
            if (m_cell[i].NormalItemInCell != null && m_cell[i].NormalItemInCell == cell.NormalItemInCell)
            {
                return true;
            }
        }
        return false;
    }

    int GetIndexOfItemInCell(Cell cell)
    {
        for (int i = 0; i < m_cell.Count; i++)
        {
            if (m_cell[i].NormalItemInCell == cell.NormalItemInCell)
            {
                m_cell[i].NormalItemInCell = null;
                m_cellStatus[i] = false;
            }
        }
        return -1;
    }

    public void RemoveCell(Cell cell)
    {
        for (int i = 0; i < m_cell.Count; i++)
        {
            if (m_cell[i].Item == cell.Item)
            {
                m_cell[i].CellStatus = false;
                m_cell[i].Free();
                break;
            }
        }
    }

    //This use as bottom cell controller, I have no time to refactor this code, it's look quite weird
    public void FindMatchAndCollapse()
    {
        int[] count = { 0, 0, 0 };


        List<NormalItem> items = new List<NormalItem>();
        m_listNormalItem = new List<ListNormalItem>();

        eNormalType[] eNormal = new eNormalType[3];

        for (int i = 0; i < 3; i++)
        {
            m_listNormalItem.Add(new ListNormalItem());
            m_listNormalItem[i] = new ListNormalItem();
            List<NormalItem> normalItem = new List<NormalItem>();
            m_listNormalItem[i].normalItems = normalItem;
        }

        AddItemsIntolist(ref eNormal, ref items);

        //Find matches here
        for (int i = 0; i < m_cell.Count; i++)
        {

            if (m_cell[i].NormalItemInCell != null)
            {
                int index = GetIndexOfNormalItem(items, m_cell[i].NormalItemInCell);

                if (index != -1)
                {
                    count[index]++;
                    m_listNormalItem[index].normalItems.Add(m_cell[i].NormalItemInCell);
                    if (count[index] == 3)
                    {
                        foreach (var item in m_listNormalItem[index].normalItems)
                        {
                            for (int k = 0; k < m_cell.Count; k++)
                            {
                                if (m_cell[k].NormalItemInCell == item)
                                {
                                    m_cell[k].NormalItemInCell = null;
                                    m_cellStatus[k] = false;
                                }
                            }
                            item.ExplodeView();
                            m_itemsInBoad--;
                            if (m_itemsInBoad == 0)
                            {
                                m_gameManager.State = GameManager.eStateGame.GAME_WIN;
                            }
                        }
                    }


                }
            }


        }
   
    }

    int GetIndexOfNormalItem(List<NormalItem> items, NormalItem itemInlist)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].ItemType == itemInlist.ItemType)
            {
                return i;
            }
        }
        return -1;
    }

    void AddItemsIntolist(ref eNormalType[] eNormal, ref List<NormalItem> items)
    {
        for (int i = 0; i < 3; i++)
        {

            if (m_cell[i].NormalItemInCell != null && i == 0)
            {
                eNormal[i] = m_cell[i].NormalItemInCell.ItemType;
                items.Add(m_cell[i].NormalItemInCell);
            }
            else if (m_cell[i].NormalItemInCell != null && m_cell[i].NormalItemInCell.ItemType != eNormal[i - 1] && i == 1)
            {
                eNormal[i] = m_cell[i].NormalItemInCell.ItemType;
                items.Add(m_cell[i].NormalItemInCell);
            }
            else if (m_cell[i].NormalItemInCell != null && m_cell[i].NormalItemInCell.ItemType != eNormal[i - 1] && m_cell[i].NormalItemInCell.ItemType != eNormal[i - 2] && i == 2)
            {
                eNormal[i] = m_cell[i].NormalItemInCell.ItemType;
                items.Add(m_cell[i].NormalItemInCell);
            }
        }
    }

}
