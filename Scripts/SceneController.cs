using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    private Camera MainCamera;
    Vector3 mcPos;
    [SerializeField]
    private GameObject ReflectionCamera;
    [SerializeField]
    private GameObject waterPlaneBack;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private GameObject Volume;
    [SerializeField]
    private PostProcessVolume ppVolume;
    float time1;
    float time2;
    bool downOnce = true;
    bool rotateOnce = true;
    

    // Start is called before the first frame update
    void Start()
    {
        MainCamera.transform.position = new Vector3(0,-10,0);
        MainCamera.transform.eulerAngles = new Vector3(-45f,0,0);
        mcPos = MainCamera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        mcPos = MainCamera.transform.position;

        if (mcPos.y >= 0 && mcPos.y< 3)
        {
            if (!ReflectionCamera.activeSelf)
            {
                ReflectionCamera.SetActive(true);
            }

            if (waterPlaneBack.activeSelf)
            {
                waterPlaneBack.SetActive(false);
            }

            if (audioSource.volume < 0.7f)
            {
                audioSource.volume = 0.7f;
            }

            if (audioSource.pitch < 0.5f)
            {
                audioSource.pitch = 0.5f;
            }
            
        }
        else if (mcPos.y < 0)
        {
            if (ReflectionCamera.activeSelf)
            {
                ReflectionCamera.SetActive(false);
            }

            if (!waterPlaneBack.activeSelf)
            {
                waterPlaneBack.SetActive(true);
            }

            if (mcPos.y <= -2)
            {
                audioSource.volume = ((1f/480f)*mcPos.y*mcPos.y) + ((1f/16f)*mcPos.y) + (37f/60f);
                //audioSource.pitch = ((1f/480f) * mcPos.y * mcPos.y) + ((3f/80f)*mcPos.y) + (11f/30f);
            }

            MainCamera.transform.eulerAngles = new Vector3(-55f - mcPos.y, 0, 0);

        }

        if (time1 <= 5f)
        {
            time1 += Time.deltaTime;
        }
        

        //if (Input.GetKey(KeyCode.L))
        if(time1 >= 5f)
        {
            if (downOnce)
            {
                MainCamera.transform.position -= new Vector3(0, Time.deltaTime, 0);
                if (mcPos.y < -12f)
                {
                    downOnce = false;
                }
                return;
            }

            if(mcPos.y < -2f)
            {
                MainCamera.transform.position += new Vector3(0, Time.deltaTime * 5, 0);
            }
            else if (mcPos.y < 0f && mcPos.y >=-2f)
            {
                MainCamera.transform.position += new Vector3(0, Time.deltaTime * 2, 0);
            }
            else if(mcPos.y >= 0f && mcPos.y < 3f)
            {
                MainCamera.transform.position += new Vector3(0, Time.deltaTime * 5, 0);
            }
            else if (mcPos.y >= 3f)
            {
                if (time2 <= 2f)
                {
                    time2 += Time.deltaTime;
                    return;
                }

                if (rotateOnce)
                {
                    float des = 360f;
                    float curMCAngleX = MainCamera.transform.eulerAngles.x;
                    curMCAngleX = Mathf.Lerp(curMCAngleX, des, Time.deltaTime*0.3f);
                    MainCamera.transform.eulerAngles = new Vector3(curMCAngleX, 0, 0);
                    if (MainCamera.transform.eulerAngles.x - des >= -0.1f && MainCamera.transform.eulerAngles.x - des <= 0.1f)
                    {
                        MainCamera.transform.eulerAngles = new Vector3(360, 0, 0);
                        rotateOnce = false;
                    }
                }
                else if (!rotateOnce)
                {
                    if (!Volume.activeSelf)
                    {
                        Volume.SetActive(true);
                    }
                    ppVolume.weight += Time.deltaTime * 0.065f;
                    audioSource.volume -= Time.deltaTime * 0.05f;


                }
            }
        }




        
    }
}
