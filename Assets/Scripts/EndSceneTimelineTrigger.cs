using UnityEngine;
using UnityEngine.Playables;

public class EndSceneTimelineTrigger : MonoBehaviour
{
    public PlayableDirector director;

    void Start()
    {
        if (director != null)
        {
            director.Play(); // Play Timeline directly without waiting for fade
        }
    }
}
