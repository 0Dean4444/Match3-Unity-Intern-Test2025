using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public bool IsBusy { get; private set; }

    private Board m_board;
    private GameManager m_gameManager;
    private GameSettings m_gameSettings;
    private Camera m_cam;
    private bool m_gameOver;
    private List<Item> m_tray = new List<Item>();
    public void StartGame(GameManager gameManager, GameSettings gameSettings, bool autoWin = false, bool autoLose = false)
    {
        m_gameManager = gameManager;
        m_gameSettings = gameSettings;
        m_gameManager.StateChangedAction += OnGameStateChange;
        m_cam = Camera.main;
        m_board = new Board(this.transform, gameSettings);
        m_board.Fill();
        if (autoWin) StartCoroutine(AutoplayCoroutine(true));
        else if (autoLose) StartCoroutine(AutoplayCoroutine(false));
    }
    private void OnGameStateChange(GameManager.eStateGame state)
    {
        if (state == GameManager.eStateGame.GAME_OVER_WIN || state == GameManager.eStateGame.GAME_OVER_LOSE)
            m_gameOver = true;
    }
    public void Update()
    {
        if (m_gameOver || IsBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = m_cam.ScreenToWorldPoint(Input.mousePosition);
            if (m_gameManager.IsAttackMode)
            {
                bool clickedTray = false;
                for (int i = m_tray.Count - 1; i >= 0; i--)
                {
                    if (Vector2.Distance(mousePos, m_tray[i].View.position) < 0.6f)
                    {
                        ReturnItemFromTray(m_tray[i]);
                        clickedTray = true;
                        break;
                    }
                }
                if (clickedTray) return;
                if (m_tray.Count >= m_gameSettings.MaxTraySize) return;
            }
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
            if (hits.Length > 0)
            {
                Cell topmostCell = null;
                float minZ = float.MaxValue;
                foreach (var hit in hits)
                {
                    Cell cell = hit.collider.GetComponent<Cell>();
                    if (cell != null && !cell.IsEmpty && cell.IsClickable)
                    {
                        if (cell.transform.position.z < minZ)
                        {
                            minZ = cell.transform.position.z;
                            topmostCell = cell;
                        }
                    }
                }
                if (topmostCell != null) HandleItemSelection(topmostCell);
            }
        }
    }
    private void HandleItemSelection(Cell cell)
    {
        IsBusy = true;
        Item item = cell.Item;

        item.OriginalCell = cell;
        cell.Free();
        m_board.UpdateExposedItems();

        InsertIntoTray(item);
        UpdateTrayPositions();

        StartCoroutine(ProcessTray());
    }
    private void ReturnItemFromTray(Item item)
    {
        m_tray.Remove(item);
        Cell target = item.OriginalCell;

        target.Assign(item);

        item.View.DOKill();
        item.View.DOJump(target.transform.position, 1.5f, 1, 0.5f);

        UpdateTrayPositions();
        m_board.UpdateExposedItems();
    }
    private void InsertIntoTray(Item newItem)
    {
        int insertIndex = m_tray.Count;
        for (int i = 0; i < m_tray.Count; i++)
        {
            if (m_tray[i].IsSameType(newItem)) insertIndex = i + 1;
        }
        m_tray.Insert(insertIndex, newItem);
    }
    private void UpdateTrayPositions()
    {
        float spacing = 1.2f;
        float startX = -(m_gameSettings.MaxTraySize - 1) * spacing / 2f;
        Vector3 trayCenterPos = new Vector3(startX, -4.5f, 0f);

        for (int i = 0; i < m_tray.Count; i++)
        {
            Vector3 targetPos = trayCenterPos + new Vector3(i * spacing, 0, 0);

            m_tray[i].View.DOKill();
            m_tray[i].View.localScale = Vector3.one;

            if (Vector3.Distance(m_tray[i].View.position, targetPos) > 1.5f)
            {
                m_tray[i].View.DOJump(targetPos, 2.5f, 1, 0.45f).SetEase(Ease.OutQuad);
            }
            else
            {
                m_tray[i].View.DOMove(targetPos, 0.2f).SetEase(Ease.OutQuad);
            }
            m_tray[i].View.GetComponent<SpriteRenderer>().sortingOrder = 1000 + i;
        }
    }
    private IEnumerator ProcessTray()
    {
        yield return new WaitForSeconds(0.5f);
        bool matched = false;
        NormalItem.eNormalType matchedType = NormalItem.eNormalType.TYPE_ONE;
        for (int i = 0; i <= m_tray.Count - 3; i++)
        {
            if (m_tray[i].IsSameType(m_tray[i + 1]) && m_tray[i].IsSameType(m_tray[i + 2]))
            {
                matched = true;
                Item i1 = m_tray[i], i2 = m_tray[i + 1], i3 = m_tray[i + 2];
                matchedType = ((NormalItem)i1).ItemType;

                m_tray.RemoveRange(i, 3);

                i1.ExplodeView(); i2.ExplodeView(); i3.ExplodeView();

                UpdateTrayPositions();

                yield return new WaitForSeconds(0.2f);
                break;
            }
        }
        if (matched)
        {
            int remainingOnBoard = m_board.AllCells.Count(c => !c.IsEmpty && ((NormalItem)c.Item).ItemType == matchedType);
            int remainingInTray = m_tray.Count(t => ((NormalItem)t).ItemType == matchedType);
            if (remainingOnBoard + remainingInTray == 0) ShowTypeClearedEffect(matchedType);
        }
        int itemsLeftOnBoard = m_board.AllCells.Count(c => !c.IsEmpty);
        if (itemsLeftOnBoard == 0 && m_tray.Count == 0)
        {
            m_gameManager.GameOver(true);
        }
        else if (m_tray.Count >= m_gameSettings.MaxTraySize && !matched)
        {
            if (!m_gameManager.IsAttackMode) m_gameManager.GameOver(false);
        }
        IsBusy = false;
    }
    private void ShowTypeClearedEffect(NormalItem.eNormalType clearedType)
    {
        NormalItem ghostItem = new NormalItem();
        ghostItem.SetType(clearedType);
        ghostItem.SetView();
        ghostItem.View.position = Vector3.zero;
        ghostItem.View.localScale = Vector3.zero;

        SpriteRenderer sr = ghostItem.View.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 2000;

        Sequence seq = DOTween.Sequence();
        seq.Append(ghostItem.View.DOScale(3f, 0.4f).SetEase(Ease.OutBack));
        seq.Append(ghostItem.View.DOPunchRotation(new Vector3(0, 0, 30f), 0.4f, 5, 1f));
        seq.Append(ghostItem.View.DOMoveY(3f, 0.5f).SetEase(Ease.InBack));
        seq.Join(sr.DOFade(0f, 0.5f));
        seq.OnComplete(() => { ghostItem.Clear(); });
    }
    private IEnumerator AutoplayCoroutine(bool goalIsWin)
    {
        yield return new WaitForSeconds(0.5f);

        while (!m_gameOver)
        {
            if (!IsBusy)
            {
                var availableCells = m_board.AllCells.Where(c => !c.IsEmpty && c.IsClickable).ToList();
                if (availableCells.Count == 0) break;

                Cell cellToPick = null;

                if (goalIsWin)
                {
                    foreach (var cell in availableCells)
                        if (m_tray.Count(t => t.IsSameType(cell.Item)) == 2) { cellToPick = cell; break; }

                    if (cellToPick == null)
                        foreach (var cell in availableCells)
                            if (m_tray.Count(t => t.IsSameType(cell.Item)) == 1 && availableCells.Count(c => c.Item.IsSameType(cell.Item)) >= 2) { cellToPick = cell; break; }

                    if (cellToPick == null)
                        foreach (var cell in availableCells)
                            if (availableCells.Count(c => c.Item.IsSameType(cell.Item)) >= 3) { cellToPick = cell; break; }

                    if (cellToPick == null)
                        foreach (var cell in availableCells)
                            if (m_tray.Any(t => t.IsSameType(cell.Item))) { cellToPick = cell; break; }

                    if (cellToPick == null) cellToPick = availableCells.OrderBy(c => c.BoardZ).First();
                }
                else
                {
                    cellToPick = availableCells.FirstOrDefault(c => !m_tray.Any(t => t.IsSameType(c.Item)));
                    if (cellToPick == null) cellToPick = availableCells[0];
                }

                if (cellToPick != null) HandleItemSelection(cellToPick);
            }
            yield return new WaitForSeconds(m_gameSettings.ActionDelay);
        }
    }
    internal void Clear()
    {
        m_board.Clear();
        foreach (var item in m_tray) if (item != null) item.Clear();
        m_tray.Clear();
    }
}