using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPlaneMove : MonoBehaviour
{
    [SerializeField]
    private Camera MainCamera;
    Vector3 mcPos;
    Vector3 mcAngle;
    // Start is called before the first frame update
    void Start()
    {
        mcPos = MainCamera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        mcPos = MainCamera.transform.position;
        transform.position = new Vector3(mcPos.x, transform.position.y, mcPos.z);
    }
}
