using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


public class IceDrop : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Drop()
    {
        rb.useGravity = true;
    }

}
