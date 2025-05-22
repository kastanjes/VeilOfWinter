using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotherStandTrigger : MonoBehaviour
{
    public Animator motherAnimator;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StandTrigger()
    {
        motherAnimator.SetTrigger("StandUp");
    }

        public void WalkTrigger()
    {
        motherAnimator.SetTrigger("Walk");
    }
}
