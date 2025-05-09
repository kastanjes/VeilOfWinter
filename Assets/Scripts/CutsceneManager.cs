using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class CutsceneManager : MonoBehaviour
{
    public GameObject player;

    [Header("Guiding Light")]

    [SerializeField] private GuidingLightController guidingLight;



    [Header("UI")]
    public CanvasGroup blackOverlay;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI continuePrompt;
    public TextMeshProUGUI dialogueText2;
    public TextMeshProUGUI dialogueText2g;

    public CanvasGroup introUIPressE;
    

    [Header("Cameras")]
    public CinemachineVirtualCamera topDownCam;
    public CinemachineVirtualCamera sideCam;
    public CinemachineVirtualCamera guidingLightCam;

    [Header("Dialogue")]
    public float typeSpeed = 0.01f;

    [Header("Intro Torch")]
    public IntroTorchController introTorch; // assign in inspector
    public GameObject PlayerTorch;

    [Header("Vignette Fade")]
    public CanvasGroup blackVignetteOverlay; // assign your soft hole black image

    [Header("Vignette Settings")]
    public RectTransform vignetteTransform;
    public Vector3 startScale = new Vector3(366, 184, 366);
    public Vector3 endScale = new Vector3(20, 10, 30);

    public CanvasGroup menuCanvas;  // assign your menu canvas here

    public Animator playerAnimator;

    private bool cutsceneStarted = false;

    public GameObject windzone;

    public GameObject plane;

    private Quaternion originalPlayerRotation;



    private void Start()
    {
        originalPlayerRotation = player.transform.rotation;
        player.GetComponent<CharacterMovement>().enabled = false;
        player.GetComponent<Rigidbody>().isKinematic = true;

        guidingLight.GetComponent<Rigidbody>().isKinematic = true;


        blackOverlay.gameObject.SetActive(false);
        windzone.gameObject.SetActive(false);
    }

    public void StartCutscene()
    {
        
        if (!cutsceneStarted)
        {
            cutsceneStarted = true;
            Debug.Log("Play button pressed - starting cutscene");


            // Fade menu
            StartCoroutine(FadeOutMenu());

            StartCoroutine(CutsceneSequence());
        }
    }


    IEnumerator CutsceneSequence()
    {
        dialogueText.gameObject.SetActive(false);
        continuePrompt.gameObject.SetActive(false);

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
        yield return FadeBlack(true);

        yield return new WaitForSeconds(2f);
        
        dialogueText.gameObject.SetActive(true);
        continuePrompt.gameObject.SetActive(true);

        windzone.gameObject.SetActive(true);
        // plane.gameObject.SetActive(false);

        RenderSettings.fogColor = new Color(0f / 255f, 1f / 255f, 25f / 255f);

        foreach (string line in introDialogueLines)
        {
            yield return ShowDialogue(line);
        }
        // STAND UP
playerAnimator.SetTrigger("StandingUp");

        // FADE BACK IN
        yield return FadeBlackOutSlow();


// STAND UP
playerAnimator.SetTrigger("StandingUp");

// Wait until "Standing up" state is actually active
while (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Standing up"))
{
    yield return null;
}

// Then wait for it to finish
while (playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f || playerAnimator.IsInTransition(0))
{
    yield return null;
}

// Only now do we trigger the next animation
playerAnimator.SetTrigger("MovePlayer");

// Wait until the state is in "MovePlayer" and no longer in transition
while (playerAnimator.IsInTransition(0) ||
       !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("WalkingPickupTorch 0"))
{
    yield return null;
}

// Begin moving the player AND wait until the coroutine is done
yield return StartCoroutine(MovePlayerAlongCutscenePath(playerAnimator.transform));

// Set player to idle animation AFTER movement
// playerAnimator.SetTrigger("IdleLoop");

// Start guiding light dialogue
dialogueText2.gameObject.SetActive(true);
continuePrompt.gameObject.SetActive(true);

yield return ShowDialogue("??? : Follow the lights.", "GuidingLight");
yield return ShowDialogue("Boy : Who... who's there?", "Player");
yield return ShowDialogue("??? : I'll lead you home.", "GuidingLight");
yield return ShowDialogue("Boy : ...but, who ar-", "Player");


// (Optional) disable text afterwards
dialogueText2.gameObject.SetActive(false);
continuePrompt.gameObject.SetActive(false);

guidingLight.GetComponent<GuidingLightPath>().enabled = false;


StartCoroutine(guidingLight.MoveToNextWaypoint());

    yield return new WaitForSeconds(5f);
    sideCam.Priority = 20;
    guidingLightCam.Priority = 0;


    player.GetComponent<Rigidbody>().isKinematic = false;
    player.GetComponent<CharacterMovement>().enabled = true;
    playerAnimator.SetTrigger("StartGame");



StartCoroutine(FadeInAndOutIntroUIPressE());

}


    IEnumerator MovePlayerAlongCutscenePath(Transform player)
{
    float t;

    // First movement: 0:00 to 1:00 (approx 1 sec)
    t = 0f;
    Vector3 p0 = new Vector3(-0.16891f, -0.15f, -0.5696642f);
    Vector3 p1 = new Vector3(1.17118f, -0.4566295f, 0.6187807f);
    while (t < 1f)
    {
        t += Time.deltaTime;
        player.position = Vector3.Lerp(p0, p1, t / 1f);
        yield return null;
    }

    // 1:00 to 1:30 (0.5 sec)
    t = 0f;
    Vector3 p2 = new Vector3(1.481089f, -0.4566295f, 0.9715534f);
    while (t < 0.5f)
    {
        t += Time.deltaTime;
        player.position = Vector3.Lerp(p1, p2, t / 0.5f);
        yield return null;
    }

    // 1:30 to 3:46 (2.26 sec)
    t = 0f;
    Vector3 p3 = new Vector3(1.481089f, -0.4566295f, 1.0203358f);
    while (t < 2.26f)
    {
        t += Time.deltaTime;
        player.position = Vector3.Lerp(p2, p3, t / 2.26f);
        yield return null;
    }

    // 3:46 to 4:53 (1.23 sec)
    t = 0f;
    Vector3 p4 = new Vector3(1.481089f, -0.4566295f, 3.0503358f);
    while (t < 1.23f)
    {
        t += Time.deltaTime;
        player.position = Vector3.Lerp(p3, p4, t / 1.23f);
        yield return null;
    }

    // Ensure final position is exact
    player.position = p4;

    // Switch camera to guiding light cam
    topDownCam.Priority = 0;
    sideCam.Priority = 0;
    guidingLightCam.Priority = 20;

}


IEnumerator FadeBlack(bool fadeIn)
{
    float duration = 1f;
    float t = 0f;

    // Skip fade if already at target
    if ((fadeIn && blackOverlay.alpha >= 1f) || (!fadeIn && blackOverlay.alpha <= 0f))
        yield break;

    float startAlpha = blackOverlay.alpha;
    float targetAlpha = fadeIn ? 1f : 0f;

    while (t < duration)
    {
        t += Time.deltaTime;
        blackOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
        yield return null;
    }

    blackOverlay.alpha = targetAlpha;
}

IEnumerator FadeBlackOutSlow()
{


    float duration = 3f;
    float t = 0f;
    float startAlpha = blackOverlay.alpha;
    float targetAlpha = 0f;

    while (t < duration)
    {
        t += Time.deltaTime;
        blackOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
        yield return null;
    }

    blackOverlay.alpha = 0f;
    blackOverlay.gameObject.SetActive(false); // Optional: hide it fully
}



IEnumerator ShowDialogue(string text, string speaker = "Default")
{
    // Disable all dialogue boxes first
    dialogueText.gameObject.SetActive(false);
    dialogueText2.gameObject.SetActive(false);
    dialogueText2g.gameObject.SetActive(false);
    continuePrompt.gameObject.SetActive(false);

    TextMeshProUGUI currentBox = dialogueText;

    if (speaker == "Player")
        currentBox = dialogueText2;
    else if (speaker == "GuidingLight")
        currentBox = dialogueText2g;

    currentBox.gameObject.SetActive(true);
    currentBox.text = "";

    foreach (char c in text)
    {
        currentBox.text += c;
        yield return new WaitForSeconds(typeSpeed);
    }

    continuePrompt.gameObject.SetActive(true);
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));

    // Optional: hide after click
    currentBox.gameObject.SetActive(false);
}


    private readonly string[] introDialogueLines = new string[]
    {
        "I fell asleep...",
        "My torch has gone out",
        "...",
        "What time is it?",
        "The storm has picked up",
        "...",
        "I need to get home"
    };


private readonly string[] guidingLightDialogueLines = new string[]
{
    "??? : Follow the lights.",
    "Player : Who... who's there?",
    "??? : I'll lead you home.",
    "Player : ...Okay."
};
IEnumerator FadeInAndOutIntroUIPressE()
{
    float duration = 1f;
    float t = 0f;

    // Fade in
    introUIPressE.gameObject.SetActive(true);
    introUIPressE.blocksRaycasts = true;

    while (t < duration)
    {
        t += Time.deltaTime;
        introUIPressE.alpha = Mathf.Lerp(0f, 1f, t / duration);
        yield return null;
    }

    // Wait for 5 seconds
    yield return new WaitForSeconds(10f);

    // Fade out
    t = 0f;
    while (t < duration)
    {
        t += Time.deltaTime;
        introUIPressE.alpha = Mathf.Lerp(1f, 0f, t / duration);
        yield return null;
    }

    introUIPressE.blocksRaycasts = false;
    introUIPressE.gameObject.SetActive(false); // Optional
}




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

    blackVignetteOverlay.gameObject.SetActive(false);
    
    
}







}
