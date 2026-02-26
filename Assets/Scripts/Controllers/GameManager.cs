using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eStateGame
    {
        SETUP, MAIN_MENU, GAME_STARTED, PAUSE, GAME_OVER_WIN, GAME_OVER_LOSE
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set { m_state = value; StateChangedAction(m_state); }
    }

    private GameSettings m_gameSettings;
    private BoardController m_boardController;
    private UIMainManager m_uiMenu;
    public bool IsAttackMode { get; private set; }
    public float AttackTimer { get; private set; }

    private void Awake()
    {
        State = eStateGame.SETUP;
        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);
        m_uiMenu = FindAnyObjectByType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start() { State = eStateGame.MAIN_MENU; }

    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
        if (State == eStateGame.GAME_STARTED && IsAttackMode)
        {
            AttackTimer -= Time.deltaTime;
            if (AttackTimer <= 0)
            {
                AttackTimer = 0;
                GameOver(false);
            }
        }
    }

    internal void SetState(eStateGame state)
    {
        State = state;
        if (State == eStateGame.PAUSE) DOTween.PauseAll();
        else DOTween.PlayAll();
    }

    public void LoadGame(bool isAutoPlayWin = false, bool isAutoPlayLose = false, bool isAttackMode = false)
    {
        IsAttackMode = isAttackMode;
        AttackTimer = 60f;

        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings, isAutoPlayWin, isAutoPlayLose);

        State = eStateGame.GAME_STARTED;
    }

    public void GameOver(bool isWin)
    {
        StartCoroutine(WaitBoardController(isWin));
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }

    private IEnumerator WaitBoardController(bool isWin)
    {
        while (m_boardController.IsBusy) yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.5f);
        State = isWin ? eStateGame.GAME_OVER_WIN : eStateGame.GAME_OVER_LOSE;
    }
}