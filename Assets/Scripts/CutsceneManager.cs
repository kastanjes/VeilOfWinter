using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup blackOverlay;
    public TextMeshProUGUI dialogueText;

    [Header("Cameras")]
    public CinemachineVirtualCamera topDownCam;
    public CinemachineVirtualCamera sideCam;

    [Header("Dialogue")]
    public float typeSpeed = 0.04f;

    [Header("Intro Torch")]
    public IntroTorchController introTorch; // assign in inspector

    [Header("Vignette Fade")]
    public CanvasGroup blackVignetteOverlay; // assign your soft hole black image

    [Header("Vignette Settings")]
    public RectTransform vignetteTransform;
    public Vector3 startScale = new Vector3(366, 184, 366);
    public Vector3 endScale = new Vector3(20, 10, 30);



    public Animator playerAnimator;


    private bool cutsceneStarted = false;


    private void Start()
    {
        blackOverlay.gameObject.SetActive(false);
    }

    public void StartCutscene()
    {
        if (!cutsceneStarted)
        {
            cutsceneStarted = true;

            // Fade menu
            StartCoroutine(FadeOutMenu());

            StartCoroutine(CutsceneSequence());
        }
    }


    IEnumerator CutsceneSequence()
    {
        // Start with top-down cam enabled
        topDownCam.Priority = 20;
        sideCam.Priority = 10;

        yield return new WaitForSeconds(1f);

        // PAN CAMERA: switch to side view smoothly
        topDownCam.Priority = 10;
        sideCam.Priority = 20;

        yield return new WaitForSeconds(2f); // let the pan finish

        // FADE TO BLACK
        // Start torch fade and vignette fade together
        introTorch.FadeOutTorch();
        yield return FadeVignetteToBlack(introTorch.fadeDuration); 
 // same duration as torch fade


        // SHOW DIALOGUE
        yield return ShowDialogue("Huh... I fell asleep...");
        yield return ShowDialogue("What time is it? The storm is picking up.");
        yield return ShowDialogue("I need to get home.");

        // FADE BACK IN
        yield return FadeBlack(false);

        // STAND UP
        playerAnimator.SetTrigger("StandUp");

        yield return new WaitForSeconds(2.2f); // wait for animation
    }

    IEnumerator FadeBlack(bool fadeIn)
    {
        float duration = 1f;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            blackOverlay.alpha = fadeIn ? Mathf.Lerp(0, 1, t / duration) : Mathf.Lerp(1, 0, t / duration);
            yield return null;
        }
    }

    IEnumerator ShowDialogue(string text)
    {
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
    }

    public CanvasGroup menuCanvas;  // assign your menu canvas here

IEnumerator FadeOutMenu()
{
    float duration = 1f;
    float t = 0;
    while (t < duration)
    {
        t += Time.deltaTime;
        menuCanvas.alpha = Mathf.Lerp(1, 0, t / duration);
        yield return null;
    }

    menuCanvas.interactable = false;
    menuCanvas.blocksRaycasts = false;
    menuCanvas.gameObject.SetActive(false);  // optional: fully disable it
}
IEnumerator FadeVignetteToBlack(float duration)
{
    float t = 0f;

    // Vignette fading and scaling during torch fade
    while (t < duration)
    {
        t += Time.deltaTime;
        float progress = t / duration;

        blackVignetteOverlay.alpha = Mathf.Lerp(0, 1, progress);
        vignetteTransform.localScale = Vector3.Lerp(startScale, endScale, progress);

        yield return null;
    }

    // Snap to final values
    blackVignetteOverlay.alpha = 1;
    vignetteTransform.localScale = endScale;

    // Now quickly fade in the solid black overlay
    blackOverlay.gameObject.SetActive(true);
    blackOverlay.alpha = 0;

    float fadeTime = 0.3f;
    float fadeT = 0f;
    while (fadeT < fadeTime)
    {
        fadeT += Time.deltaTime;
        blackOverlay.alpha = Mathf.Lerp(0, 1, fadeT / fadeTime);
        yield return null;
    }

    blackOverlay.alpha = 1;
}







}
