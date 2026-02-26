using UnityEngine;
using UnityEngine.UI;

public class UIPanelGame : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnPause;
    [SerializeField] private Text txtTimer;

    private UIMainManager m_mngr;
    private GameManager m_gameManager;

    private void Awake()
    {
        if (btnPause) btnPause.onClick.AddListener(OnClickPause);
    }

    private void Update()
    {
        if (m_gameManager != null && m_gameManager.IsAttackMode && txtTimer != null)
        {
            txtTimer.text = "TIME: " + Mathf.CeilToInt(m_gameManager.AttackTimer).ToString() + "s";
        }
    }

    private void OnClickPause()
    {
        m_mngr.ShowPauseMenu();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
        m_gameManager = FindAnyObjectByType<GameManager>();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        if (txtTimer != null) txtTimer.gameObject.SetActive(m_gameManager.IsAttackMode);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}