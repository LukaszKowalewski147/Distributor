using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectLemon : MonoBehaviour
{
    [SerializeField] private Image actionE;

    private InputSystem_Actions inputActions;
    private PlayerStats playerStats;
    private PlayerMovement playerMovement;
    private AnimationManager animationManager;
    private GameObject currentLemonTree;

    private bool isInterracting;
    private bool isCloseToLemons;
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

        isInterracting = false;
        isCloseToLemons = false;
        isPerformingCollect = false;
        currentLemonTree = null;
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
        if (isCloseToLemons && isInterracting && !isPerformingCollect)
        {
           PerformCollectLemon();
        }
    }

    private void PerformCollectLemon()
    {
        isInterracting = false;
        isPerformingCollect = true; // Blokuj kolejne akcje zbierania
        actionE.gameObject.SetActive(false);
        StartCoroutine(CollectLemonCoroutine());
    }

    private IEnumerator CollectLemonCoroutine()
    {
        // Zablokuj ruch postaci
        playerMovement.DisableMovement();

        // Obrót w stronê drzewa cytrynowego
        Vector3 treePosition = currentLemonTree.transform.position;
        playerMovement.RotateToTarget(treePosition);

        // Aktualizacja animacji
        animationManager.Collect();

        // Czekaj 3 sekundy
        yield return new WaitForSeconds(3f);

        // Dodaj cytrynê do statystyk
        playerStats.AddLemon();


        if (isCloseToLemons)
        {
            actionE.gameObject.SetActive(true);
        }

        // Odblokuj ruch postaci
        playerMovement.EnableMovement();

        isPerformingCollect = false; // Odblokuj zbieranie
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Lemon"))
        {
            actionE.gameObject.SetActive(true);
            currentLemonTree = other.gameObject;
            isCloseToLemons = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Lemon"))
        {
            actionE.gameObject.SetActive(false);
            isCloseToLemons = false;
            currentLemonTree = null;
        }
    }
    void OnDestroy()
    {
        inputActions.Dispose();
    }
}
