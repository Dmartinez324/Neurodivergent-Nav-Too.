using System.Collections;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class ConfettiController : MonoBehaviour
{
    public GameObject target;
    public GameObject confettiPrefab;
    public GameObject canvasPrefab;
    private Transform userHeadset;
    private GameObject confettiSystemInstance;
    private GameObject celebrationCanvasInstance;

    private void Start()
    {
        // Find the user's headset
        userHeadset = target.transform;
    }

    private bool isRunning = false;

    private void Update()
    {
        // Check for Oculus headset availability
        if (userHeadset == null)
        {
            Debug.LogError("Oculus headset not found!");
            return;
        }

        // Check for user input to instantiate confetti
        //if (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.Space))
        //{
        //    InstantiateConfetti();
        //}
    }

    public void InstantiateConfetti()
    {
        if (isRunning) return;

        isRunning = true;
        userHeadset = target.transform;
        // Commented out: Instantiate confetti system at user's position with headset's rotation
        // confettiSystemInstance = Instantiate(confettiPrefab, userHeadset.position, userHeadset.rotation);
        
        // New logic: Instantiate confetti system at the origin (0, 0, 0) for visibility in the editor
        confettiSystemInstance = Instantiate(confettiPrefab, new Vector3(userHeadset.position.x, userHeadset.position.y + 10, userHeadset.position.z), Quaternion.Euler(90,0,0));
        // Vector3 canvasPos = userHeadset.position + userHeadset.forward * 1f + new Vector3(0, -0.5f, 0);
        Vector3 canvasPos = userHeadset.position + userHeadset.forward * 1f;
        // canvasPos.y = userHeadset.position.y;
        Quaternion canvasRot = Quaternion.Euler(0, userHeadset.rotation.eulerAngles.y, 0);
        celebrationCanvasInstance = Instantiate(canvasPrefab, canvasPos, canvasRot);

        // Start coroutine to destroy confetti after 5 seconds
        StartCoroutine(DestroyConfettiAfterDelay(5f));
    }

    public IEnumerator DestroyConfettiAfterDelay(float delay)
    {
        // Wait for specified time
        yield return new WaitForSeconds(delay);

        // Stop and destroy confetti system
        if (confettiSystemInstance != null)
        {
            isRunning = false;
            confettiSystemInstance.GetComponent<ParticleSystem>().Stop();
            Destroy(confettiSystemInstance, 2f); // Give some time for the confetti to settle before destroying
        }

        if(celebrationCanvasInstance != null)
        {
            Destroy(celebrationCanvasInstance);
        }
    }
}
