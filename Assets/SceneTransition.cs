using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("遷移先")]
    [SerializeField] private string nextSceneName;

    [Header("自動遷移")]
    [SerializeField] private bool autoTransition = false;
    [SerializeField] private float transitionDelay = 5f;

    private bool isTransitioning = false;

    void Update()
    {
        if (isTransitioning || Keyboard.current == null) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ChangeScene(nextSceneName);
        }
    }

    void Start()
    {
        if (autoTransition)
        {
            StartCoroutine(TransitionAfterDelay());
        }
    }

    IEnumerator TransitionAfterDelay()
    {
        yield return new WaitForSeconds(transitionDelay);
        ChangeScene(nextSceneName);
    }

    public void ChangeScene(string sceneName)
    {
        if (isTransitioning) return;

        isTransitioning = true;
        SceneManager.LoadScene(sceneName);
    }
}