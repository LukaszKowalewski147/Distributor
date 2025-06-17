using UnityEngine;
using DialogueEditor;

public class ConversatePavelek : MonoBehaviour
{
    [SerializeField] private NPCConversation conversation;

    private TalkInteractionManager interacionManager;
    private ConversationManager conversationManager;
    private PlayerStats playerStats;

    private readonly int lemonsToPay = 5;

    private void Start()
    {
        interacionManager = null;
        conversationManager = null;
        playerStats = null;
    }

    private void Update()
    {
        if (conversationManager != null)
        {
            if (!conversationManager.IsConversationActive)
            {
                interacionManager.EndInteraction();
                interacionManager = null;
                conversationManager = null;
                playerStats = null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainPlayer"))
        {
            GameObject player = other.gameObject;
            playerStats = player.GetComponent<PlayerStats>();
            interacionManager = player.GetComponent<TalkInteractionManager>();

            interacionManager.PrepareForInteractionWithTarget(transform.position);

            conversationManager = ConversationManager.Instance;
            conversationManager.StartConversation(conversation);

            CheckOwnedLemons();
        }
    }

    public void GivePavelekLemons()
    {
        playerStats.TakeAwayLemons(lemonsToPay);
    }

    public void PrepareForTripToForest()
    {
        interacionManager.PrepareForTripToForest();
    }

    public void PrepareForTripToDessert()
    {
        interacionManager.PrepareForTripToDessert();
    }

    private void CheckOwnedLemons()
    {
        int lemonsOwned = playerStats.GetLemonsOwned();
        bool hasEnoughLemons = lemonsOwned >= lemonsToPay;

        conversationManager.SetBool("hasEnoughLemons", hasEnoughLemons);
    }
}
