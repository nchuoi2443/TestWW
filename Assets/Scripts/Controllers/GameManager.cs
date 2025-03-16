using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;
using UnityEngine.Analytics;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public event Action<eStateGame> StateChangedAction = delegate { };

    

    public enum eLevelMode
    {
        TIMER,
        MOVES
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        GAME_WIN,
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;


    private BoardController m_boardController;

    //test, xóa sau
    private BottomCell m_bottomCell;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    public GameObject UIGameOver;

    public GameObject UIGameWin;

    public eLevelMode CurrentGameMode { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        m_bottomCell = new GameObject("BottomCell").AddComponent<BottomCell>();
        m_bottomCell = new BottomCell(m_bottomCell.transform, m_gameSettings);

        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings, m_bottomCell);

        if (mode == eLevelMode.MOVES)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController);
        }
        else if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), this);
        }

        m_levelCondition.ConditionCompleteEvent += GameOver;

        CurrentGameMode = mode;
        State = eStateGame.GAME_STARTED;
    }

    public IEnumerator Win()
    {
        yield return new WaitForSeconds(1f);

        UIGameWin.SetActive(true);
        State = eStateGame.GAME_OVER;
    }

    public IEnumerator GameLoss()
    {
        yield return new WaitForSeconds(1f);

        UIGameOver.SetActive(true);
        State = eStateGame.GAME_OVER;
    }

    public void GameOver()
    {
        StartCoroutine(WaitBoardController());
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

    private IEnumerator WaitBoardController()
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = eStateGame.GAME_OVER;

        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= GameOver;

            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }
}
