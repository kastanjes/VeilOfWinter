using System.Collections;
using UnityEngine;
using TMPro;

public class DialogScript : MonoBehaviour
{
    public static DialogScript Instance;

    public TextMeshProUGUI textDisplay;
    public GameObject continueButton;
    public Animator textDisplayAnim;

    public AudioSource source;

    public float typingSpeed = 0.05f;

    private string[] sentences;
    private int index;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (sentences != null && index < sentences.Length && textDisplay.text == sentences[index])
        {
            continueButton.SetActive(true);
        }
    }

    public void StartDialogue(string[] newSentences)
    {
        sentences = newSentences;
        index = 0;
        textDisplay.text = "";
        StartCoroutine(Type());
    }

    IEnumerator Type()
    {
        foreach (char letter in sentences[index].ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void NextSentence()
    {
        source.Play();
        textDisplayAnim.SetTrigger("change"); 
        continueButton.SetActive(false);

        if (index < sentences.Length - 1)
        {
            index++;
            textDisplay.text = "";
            StartCoroutine(Type());
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        textDisplay.text = "";
        continueButton.SetActive(false);
    }
}
