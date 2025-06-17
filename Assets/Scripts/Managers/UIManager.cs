using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private TextMeshProUGUI hpCounter;
    [SerializeField] private TextMeshProUGUI lemonCounter;
    [SerializeField] private TextMeshProUGUI cactusNeedlesCounter;

    private InputSystem_Actions inputActions;

    private bool isMenuRequested;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.UI.Menu.performed += ctx => isMenuRequested = true;
    }

    private void Start()
    {
        isMenuRequested = false;
    }

    void OnEnable()
    {
        inputActions.UI.Enable();
    }

    void OnDisable()
    {
        inputActions.UI.Disable();
    }

    private void Update()
    {
        if (isMenuRequested)
        {
            isMenuRequested = false;
            OpenClosePauseMenu(!pauseMenu.activeSelf); // zamknij jesli otwarte i odwrotnie
        }
    }

    public void UpdateHpCounter(int hpAmount)
    {
        hpCounter.text = hpAmount.ToString();
    }

    public void UpdateLemonCounter (int lemonsAmount)
    {
        lemonCounter.text = lemonsAmount.ToString();
    }

    public void UpdateCactusNeedlesCounter(int cactusNeedlesAmount)
    {
        cactusNeedlesCounter.text = cactusNeedlesAmount.ToString();
    }

    public void OpenClosePauseMenu(bool active)
    {
        pauseMenu.SetActive(active);
        Cursor.visible = active;
    }

    void OnDestroy()
    {
        inputActions.Dispose();
    }
}
