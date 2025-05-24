// Assets/Scripts/UIButtonActions.cs
using UnityEngine;

public class UIButtonActions : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit button clicked");
    }

    public void ShowOptions()
    {
        Debug.Log("Options button clicked");
        // Show options menu here
    }
}
