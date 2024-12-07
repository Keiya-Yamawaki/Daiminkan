using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool gameStarting = true;
    public GameObject mainCamera;
    public HandContainer doraContainer;
    public static Vector3 firstPlayerCameraPosition = new Vector3(0f, 0f, 0f);
    public static Quaternion firstPlayerCameraRotation = Quaternion.Euler(0f, 0f, 0f);
    public static Vector3 playerCameraPosition0 = new Vector3(0f, 3.75f, -3f);
    public static Quaternion playerCameraRotation0 = Quaternion.Euler(60f, 0f, 0f);
    public static Vector3 playerCameraPosition1 = new Vector3(3f, 3.75f, 0f);
    public static Quaternion playerCameraRotation1 = Quaternion.Euler(60f, -90f, 0f);
    public static Vector3 playerCameraPosition2 = new Vector3(0f, 3.75f, 3f);
    public static Quaternion playerCameraRotation2 = Quaternion.Euler(60f, -180f, 0f);
    public static Vector3 playerCameraPosition3 = new Vector3(-3f, 3.75f, 0f);
    public static Quaternion playerCameraRotation3 = Quaternion.Euler(60f, -270f, 0f);
    public static Vector3 doraCameraPosition_y = new Vector3(0f, 0.95f, 0f);
    public static Quaternion doraCameraRotation = Quaternion.Euler(60f, 0f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayingCameraTransform(int myOrder)
    {
        if (gameStarting)
        {
            switch (myOrder)
            {
                case 0:
                    firstPlayerCameraPosition = playerCameraPosition0;
                    firstPlayerCameraRotation = playerCameraRotation0;
                    gameStarting = false;
                    break;
                case 1:
                    firstPlayerCameraPosition = playerCameraPosition1;
                    firstPlayerCameraRotation = playerCameraRotation1;
                    gameStarting = false;
                    break;
                case 2:
                    firstPlayerCameraPosition = playerCameraPosition2;
                    firstPlayerCameraRotation = playerCameraRotation2;
                    gameStarting = false;
                    break;
                case 3:
                    firstPlayerCameraPosition = playerCameraPosition3;
                    firstPlayerCameraRotation = playerCameraRotation3;
                    gameStarting = false;
                    break;
                default:
                    //Debug.Log("ÉGÉâÅ[");
                    break;
            }
        }
        mainCamera.transform.position = firstPlayerCameraPosition;
        mainCamera.transform.rotation = firstPlayerCameraRotation;
        gameStarting = false;
    }

    public void DoraCameraTransform(int doraCounter)
    {
        Vector3 doraCameraPosition = doraCameraPosition_y;
        doraCameraPosition.x = doraContainer[doraCounter].gameObject.transform.position.x;
        doraCameraPosition.z 
            = doraContainer[doraCounter].gameObject.transform.position.z - 0.3f;
        
        mainCamera.transform.position = doraCameraPosition;
        mainCamera.transform.rotation = doraCameraRotation;
    }
}
