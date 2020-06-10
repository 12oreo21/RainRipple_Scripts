using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncWaterPlane : MonoBehaviour
{
    [SerializeField]
    private GameObject waterPlane;
    void Awake()
    {
        transform.position = waterPlane.transform.position;
        transform.localRotation = waterPlane.transform.localRotation;
        transform.localScale = waterPlane.transform.localScale;
    }

    void Update()
    {
        transform.position = waterPlane.transform.position;
        transform.localRotation = waterPlane.transform.localRotation;
        transform.localScale = waterPlane.transform.localScale;
    }
}
