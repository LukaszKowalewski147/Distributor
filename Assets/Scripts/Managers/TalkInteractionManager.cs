using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class TalkInteractionManager : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController cameraInputController;
    private PlayerMovement playerMovement;

    private bool wannaGoToForest;
    private bool wannaGoToDessert;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        wannaGoToForest = false;
        wannaGoToDessert = false;
    }

    public void PrepareForInteractionWithTarget(Vector3 target)
    {
        cameraInputController.enabled = false;

        playerMovement.DisableMovement();
        playerMovement.SetIdleAnimation();
        playerMovement.RotateToTarget(target);
        Cursor.visible = true;

    }

    public void EndInteraction()
    {
        Cursor.visible = false;

        if (wannaGoToForest)
        {
            wannaGoToForest = false;
            GoToForest();
            return;
        }

        if (wannaGoToDessert)
        {
            wannaGoToDessert = false;
            GoToDesert();
            return;
        }

        cameraInputController.enabled = true;
        playerMovement.EnableMovement();
    }

    public void PrepareForTripToForest()
    {
        wannaGoToForest = true;
    }

    public void PrepareForTripToDessert()
    {
        wannaGoToDessert = true;
    }

    private void GoToForest()
    {
        SceneManager.LoadScene(1);
    }

    private void GoToDesert()
    {
        SceneManager.LoadScene(2);
    }
}
