using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnPlayNormal;
    [SerializeField] private Button btnAutoplayWin;
    [SerializeField] private Button btnAutoLose;
    [SerializeField] private Button btnAttackMode;
    private UIMainManager m_mngr;

    private void Awake()
    {
        if (btnPlayNormal) btnPlayNormal.onClick.AddListener(() => m_mngr.LoadGameMode(false, false));
        if (btnAutoplayWin) btnAutoplayWin.onClick.AddListener(() => m_mngr.LoadGameMode(true, false));
        if (btnAutoLose) btnAutoLose.onClick.AddListener(() => m_mngr.LoadGameMode(false, true));
        if (btnAttackMode) btnAttackMode.onClick.AddListener(() => m_mngr.LoadGameMode(false, false, true));
    }
    private void OnDestroy()
    {
        if (btnPlayNormal) btnPlayNormal.onClick.RemoveAllListeners();
        if (btnAutoplayWin) btnAutoplayWin.onClick.RemoveAllListeners();
        if (btnAutoLose) btnAutoLose.onClick.RemoveAllListeners();
    }
    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }
    public void Show()
    {
        this.gameObject.SetActive(true);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}