using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueTriggerEndScene : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea] public string line;
        public float waitTime = 2f; // how long to wait after showing the line
    }

    [Header("Dialogue Setup")]
    public DialogueLine[] dialogueSequence;

    public TextMeshProUGUI playerText;
    public TextMeshProUGUI guidingLightText;

    [Header("Typing Settings")]
    public float typeSpeed = 0.03f;

    private Coroutine sequence;

    public void TriggerDialogue()
    {
        if (sequence != null)
            StopCoroutine(sequence);

        sequence = StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        playerText.gameObject.SetActive(false);
        guidingLightText.gameObject.SetActive(false);

        foreach (var entry in dialogueSequence)
        {
            TextMeshProUGUI currentText;

            // Determine which text box to use
            if (entry.speaker == "Player")
                currentText = playerText;
            else if (entry.speaker == "GuidingLight")
                currentText = guidingLightText;
            else
                currentText = playerText; // fallback/default

            // Optional: play specific voice line
            if (entry.speaker == "GuidingLight")
            {
                if (entry.line.Contains("Follow the lights") || entry.line.Contains("I'll lead you home"))
                {
                    FindObjectOfType<AudioManager>()?.PlayOneShot("GuidingLightVoice");
                }
            }

            // Clear both boxes before showing new line
            playerText.gameObject.SetActive(false);
            guidingLightText.gameObject.SetActive(false);
            currentText.text = "";
            currentText.gameObject.SetActive(true);

            // Type out line
            foreach (char c in entry.line)
            {
                currentText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }

            // Wait for custom duration
            yield return new WaitForSeconds(entry.waitTime);
        }

        // Clean up
        playerText.gameObject.SetActive(false);
        guidingLightText.gameObject.SetActive(false);
    }
}
