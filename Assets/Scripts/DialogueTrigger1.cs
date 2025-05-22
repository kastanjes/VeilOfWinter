using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueTriggerEndScene : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public float typeSpeed = 0.03f;
    [TextArea] public string dialogueLine;

    [Header("Dialogue Boxes")]
    public TextMeshProUGUI playerTextBox;
    public CanvasGroup playerCanvas;

    public TextMeshProUGUI guidingLightTextBox;
    public CanvasGroup guidingLightCanvas;

    
    public enum Speaker { Player, GuidingLight }
    public Speaker speaker;

    public void TriggerDialogue()
    {
        StartCoroutine(PlayDialogue());
    }

    private IEnumerator PlayDialogue()
    {
        // Hide both canvases first
        playerCanvas.alpha = 0;
        guidingLightCanvas.alpha = 0;

        TextMeshProUGUI activeTextBox = null;
        CanvasGroup activeCanvas = null;

        switch (speaker)
        {
            case Speaker.Player:
                activeTextBox = playerTextBox;
                activeCanvas = playerCanvas;
                break;
            case Speaker.GuidingLight:
                activeTextBox = guidingLightTextBox;
                activeCanvas = guidingLightCanvas;
                break;
        }

        activeCanvas.alpha = 1;
        activeTextBox.text = "";

        foreach (char c in dialogueLine)
        {
            activeTextBox.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));

        activeCanvas.alpha = 0;
        gameObject.SetActive(false); // Optional cleanup
    }
}
