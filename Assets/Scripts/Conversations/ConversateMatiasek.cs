using UnityEngine;
using DialogueEditor;

public class ConversateMatiasek : MonoBehaviour
{
    [SerializeField] private NPCConversation conversation;

    private TalkInteractionManager interacionManager;
    private ConversationManager conversationManager;

    private void Start()
    {
        interacionManager = null;
        conversationManager = null;
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
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainPlayer"))
        {
            GameObject player = other.gameObject;
            interacionManager = player.GetComponent<TalkInteractionManager>();

            interacionManager.PrepareForInteractionWithTarget(transform.position);

            conversationManager = ConversationManager.Instance;
            conversationManager.StartConversation(conversation);
        }
    }
}
