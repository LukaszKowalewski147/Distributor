using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectCactusNeedle : MonoBehaviour
{
    [SerializeField] private Image actionE;

    private InputSystem_Actions inputActions;
    private PlayerStats playerStats;
    private PlayerMovement playerMovement;
    private AnimationManager animationManager;
    private AudioManager audioManager;
    private GameObject currentCactus;

    private bool isInterracting;
    private bool isCloseToCactus;
    private bool isPerformingCollect;
    
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.Interact.performed += ctx => isInterracting = true;
        inputActions.Player.Interact.canceled += ctx => isInterracting = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerMovement = GetComponent<PlayerMovement>();
        animationManager = GetComponent<AnimationManager>();
        audioManager = GetComponent<AudioManager>();

        isInterracting = false;
        isCloseToCactus = false;
        isPerformingCollect = false;
        currentCactus = null;
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (isCloseToCactus && isInterracting && !isPerformingCollect)
        {
           PerformCollectCactusNeedle();
        }
    }

    private void PerformCollectCactusNeedle()
    {
        isInterracting = false;
        isPerformingCollect = true; // Blokuj kolejne akcje zbierania
        actionE.gameObject.SetActive(false);
        StartCoroutine(CollectCactusNeedleCoroutine());
    }

    private IEnumerator CollectCactusNeedleCoroutine()
    {
        // Zablokuj ruch postaci
        playerMovement.DisableMovement();

        // Obrót w stronê kaktusa
        Vector3 cactusPosition = currentCactus.transform.position;
        playerMovement.RotateToTarget(cactusPosition);

        // Aktualizacja animacji
        animationManager.Collect();

        // Czekaj 3 sekundy
        yield return new WaitForSeconds(3f);

        // Odtworz dzwiek dodania przedmiotu
        audioManager.PlayCollectItem(transform.position);

        // Dodaj ig³ê do statystyk
        playerStats.AddCactusNeedle();

        if (isCloseToCactus)
        {
            actionE.gameObject.SetActive(true);
        }

        // Odblokuj ruch postaci
        playerMovement.EnableMovement();

        isPerformingCollect = false; // Odblokuj zbieranie
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cactus"))
        {
            actionE.gameObject.SetActive(true);
            currentCactus = other.transform.GetChild(0).gameObject;
            isCloseToCactus = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cactus"))
        {
            actionE.gameObject.SetActive(false);
            isCloseToCactus = false;
            currentCactus = null;
        }
    }
    void OnDestroy()
    {
        inputActions.Dispose();
    }
}
