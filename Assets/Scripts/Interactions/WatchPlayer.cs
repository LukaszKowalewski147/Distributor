using UnityEngine;

public class WatchPlayer : MonoBehaviour
{
    private GameObject watchTarget;
    private Quaternion startingRotation;

    private void Start()
    {
        watchTarget = null;
        startingRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (watchTarget != null)
        {
            RotateToTarget();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainPlayer"))
        {
            watchTarget = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainPlayer"))
        {
            watchTarget = null;
            ResetRotation();
        }
    }

    private void RotateToTarget()
    {
        Vector3 targetDirection = watchTarget.transform.position - transform.position;
        targetDirection.y = 0; // Ignoruj oœ Y
        if (targetDirection != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        }
    }

    private void ResetRotation()
    {
        transform.rotation = startingRotation;
    }
}
