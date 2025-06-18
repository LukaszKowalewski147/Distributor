using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Jump up")]
    [SerializeField] private AudioClip jumpUp;
    [SerializeField, Range(0.0f, 1.0f)] private float jumpUpVolume = 1.0f;

    [Header("Collect item")]
    [SerializeField] private AudioClip collectItem;
    [SerializeField, Range(0.0f, 1.0f)] private float collectItemVolume = 1.0f;

    [Header("Get hit")]
    [SerializeField] private AudioClip getHit;
    [SerializeField, Range(0.0f, 1.0f)] private float getHitVolume = 1.0f;

    [Header("Leaves rustling")]
    [SerializeField] private AudioClip leavesRustling;
    [SerializeField, Range(0.0f, 1.0f)] private float leavesRustlingVolume = 1.0f;

    private readonly string logPrefix = "AudioManager: ";

    public void PlayJumpUp(Vector3 position)
    {
        if (jumpUp != null)
        {
            AudioSource.PlayClipAtPoint(jumpUp, position, jumpUpVolume);
            return;
        }
        Debug.Log(logPrefix + "AudioClip 'jumpUp' not found");
    }

    public void PlayCollectItem(Vector3 position)
    {
        if (collectItem != null)
        {
            AudioSource.PlayClipAtPoint(collectItem, position, collectItemVolume);
            return;
        }
        Debug.Log(logPrefix + "AudioClip 'collectItem' not found");
    }

    public void PlayGetHit(Vector3 position)
    {
        if (getHit != null)
        {
            AudioSource.PlayClipAtPoint(getHit, position, getHitVolume);
            return;
        }
        Debug.Log(logPrefix + "AudioClip 'getHit' not found");
    }

    public void PlayLeavesRustling(Vector3 position)
    {
        if (leavesRustling != null)
        {
            AudioSource.PlayClipAtPoint(leavesRustling, position, leavesRustlingVolume);
            return;
        }
        Debug.Log(logPrefix + "AudioClip 'leavesRustling' not found");
    }
}
