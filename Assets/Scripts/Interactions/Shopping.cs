using UnityEngine;

public class Shopping : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    private InputSystem_Actions inputActions;

    private bool isCloseToZabka;
    private bool isInterracting;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Interact.performed += ctx => isInterracting = true;
        inputActions.Player.Interact.canceled += ctx => isInterracting = false;
    }

    private void Start()
    {
        isCloseToZabka = false;
        isInterracting = false;
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        if (isCloseToZabka && isInterracting)
        {
            isInterracting = false;

            // TODO: interaction with zabka
            Debug.Log("Shopping: interaction with zabka");
            
            if (PlayerData.multiplayer)
                ServerManager.Instance.SendInteraction("request_zabka");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zabka"))
        {
            uiManager.SetInteractionEKeyActive(true);
            isCloseToZabka = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Zabka"))
        {
            uiManager.SetInteractionEKeyActive(false);
            isCloseToZabka = false;
        }
    }

    void OnDestroy()
    {
        inputActions.Dispose();
    }
}
