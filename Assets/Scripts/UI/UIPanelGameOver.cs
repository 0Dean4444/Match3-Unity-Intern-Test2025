using UnityEngine;
using UnityEngine.UI;

public class UIPanelGameOver : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnHome;
    private UIMainManager m_mngr;
    private void Awake()
    {
        if (btnHome) btnHome.onClick.AddListener(OnClickHome);
    }
    private void OnDestroy()
    {
        if (btnHome) btnHome.onClick.RemoveAllListeners();
    }
    private void OnClickHome()
    {
        m_mngr.ShowMainMenu();
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