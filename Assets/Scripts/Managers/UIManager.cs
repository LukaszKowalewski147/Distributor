using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private TextMeshProUGUI hpCounter;
    [SerializeField] private TextMeshProUGUI lemonCounter;
    [SerializeField] private TextMeshProUGUI cactusNeedlesCounter;
    [SerializeField] private Image interactionEKey;

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
        SetInteractionEKeyActive(false);
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
            SetPauseMenuActive(!pauseMenu.activeSelf); // zamknij jesli otwarte i odwrotnie
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

    public void SetPauseMenuActive(bool isActive)
    {
        pauseMenu.SetActive(isActive);
        Cursor.visible = isActive;
    }

    public void SetInteractionEKeyActive(bool isActive)
    {
        interactionEKey.gameObject.SetActive(isActive);
    }

    void OnDestroy()
    {
        inputActions.Dispose();
    }
}
