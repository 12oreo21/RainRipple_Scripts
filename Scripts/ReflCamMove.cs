using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflCamMove : MonoBehaviour
{
    [SerializeField]
    private Camera MainCamera;
    Vector3 mcPos;
    Vector3 mcAngle;
    // Start is called before the first frame update
    void Start()
    {
        mcPos = MainCamera.transform.position;
        mcAngle = MainCamera.transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        mcPos = MainCamera.transform.position;
        mcAngle = MainCamera.transform.eulerAngles;
        if (MainCamera.transform.position.y >= 0)
        {
            transform.position = new Vector3(mcPos.x, -mcPos.y, mcPos.z);
            transform.eulerAngles = new Vector3(-mcAngle.x, mcAngle.y, -mcAngle.z);
        }
        /*
        else
        {
            transform.position = mcPos;
            transform.eulerAngles = mcAngle;
        }
        */
    }
}
