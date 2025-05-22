using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class EndMenu : MonoBehaviour
{
    public GameObject whiteScreen;
    public CanvasGroup menuGroup;
    public float fadeDuration = 2f;

    private void Start()
    {
        whiteScreen.gameObject.SetActive(true);
        menuGroup.alpha = 0f;
        menuGroup.gameObject.SetActive(false);
    }

    public void ShowEndMenu()
    {
        menuGroup.gameObject.SetActive(true);
        StartCoroutine(FadeInMenu());
    }

    private System.Collections.IEnumerator FadeInMenu()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            menuGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        menuGroup.alpha = 1f;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
