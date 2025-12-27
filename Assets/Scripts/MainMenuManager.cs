using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class bruh : MonoBehaviour
{
    [SerializeField] private float delayTime = 2f;

    // These methods will be linked to your UI Buttons
    public void ClickPlay()
    {
        StartCoroutine(LoadSceneAfterDelay("Play"));
    }

    public void ClickInstructions()
    {
        StartCoroutine(LoadSceneAfterDelay("Instructions"));
    }

    public void ClickManual()
    {
        StartCoroutine(LoadSceneAfterDelay("Manual"));
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        // 1. Wait for the animation to play
        yield return new WaitForSeconds(delayTime);

        // 2. Change the scene
        SceneManager.LoadScene(sceneName);
    }
}