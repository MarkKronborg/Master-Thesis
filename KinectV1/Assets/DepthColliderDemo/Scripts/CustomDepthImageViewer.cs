﻿using UnityEngine;
using System.Collections;

public class CustomDepthImageViewer : MonoBehaviour
{
    // the KinectManager instance
    private KinectManager manager;

    // the foreground texture
    public GUITexture backgroundImage;
    private Texture2D foregroundTex;

    // rectangle taken by the foreground texture (in pixels)
    public Rect foregroundRect;
    public Vector2 foregroundOfs;

    [HideInInspector]
    public uint userId2;

    public float colliderRadius, divider;

    // game objects to contain the joint colliders
    private GameObject[] jointColliders = null, jointColliders2 = null;

    public float rectHeight = 980, rectWidth = 1920;




    void Start()
    {
        // calculate the foreground rectangle
        Rect cameraRect = Camera.main.pixelRect;


        /*foregroundOfs = new Vector2((cameraRect.width - rectWidth) / 2, (cameraRect.height - rectHeight) / 2);
        foregroundRect = new Rect(foregroundOfs.x, cameraRect.height - foregroundOfs.y, rectWidth, -rectHeight);*/

        foregroundOfs = new Vector2(0, 0);
        //foregroundRect = new Rect(foregroundOfs.x, foregroundOfs.y, Screen.width, -Screen.height/divider);

        // create joint colliders
        int numColliders = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
        jointColliders = new GameObject[numColliders];
        jointColliders2 = new GameObject[numColliders];

        for (int i = 0; i < numColliders; i++)
        {
            string sColObjectName = ((KinectWrapper.NuiSkeletonPositionIndex)i).ToString() + "Collider";
            jointColliders[i] = new GameObject(sColObjectName);
            jointColliders[i].transform.parent = transform;

            string sColObjectName2 = ((KinectWrapper.NuiSkeletonPositionIndex)i).ToString() + "Collider2";

            jointColliders2[i] = new GameObject(sColObjectName2);
            jointColliders2[i].transform.parent = transform;

            SphereCollider collider = jointColliders[i].AddComponent<SphereCollider>();
            SphereCollider collider2 = jointColliders2[i].AddComponent<SphereCollider>();
            collider.radius = colliderRadius;
            collider2.radius = colliderRadius;
        }
    }

    void Update()
    {

        foregroundRect = new Rect(foregroundOfs.x, foregroundOfs.y, Screen.width, -Screen.height / divider);

        if (manager == null)
        {
            manager = KinectManager.Instance;
        }

        // get the users texture
        if (manager && manager.IsInitialized())
        {
            //foregroundTex = manager.GetUsersLblTex();
            if (backgroundImage && (backgroundImage.texture == null))
            {
                backgroundImage.texture = manager.GetUsersClrTex();
            }
        }

        if (manager.IsUserDetected())
        {
            uint userId = manager.GetPlayer1ID();

            //Player 2
            userId2 = manager.GetPlayer2ID();

            print(userId);
            print(userId2);

            

            // update colliders
            int numColliders = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;

            for (int i = 0; i < numColliders; i++)
            {
                if (manager.IsJointTracked(userId, i))
                {
                    Vector3 posJoint = manager.GetRawSkeletonJointPos(userId, i);

                    if (posJoint != Vector3.zero)
                    {
                        // convert the joint 3d position to depth 2d coordinates
                        Vector2 posDepth = manager.GetDepthMapPosForJointPos(posJoint);

                        float scaledX = posDepth.x * foregroundRect.width / KinectWrapper.Constants.DepthImageWidth;
                        float scaledY = posDepth.y * -foregroundRect.height / KinectWrapper.Constants.DepthImageHeight;

                        float screenX = foregroundOfs.x + scaledX;
                        float screenY = Camera.main.pixelHeight - (foregroundOfs.y + scaledY);
                        float zDistance = posJoint.z - Camera.main.transform.position.z;

                        Vector3 posScreen = new Vector3(screenX, screenY, zDistance);
                        Vector3 posCollider = Camera.main.ScreenToWorldPoint(posScreen);

                        jointColliders[i].transform.position = posCollider;
                    }
                }

                //player 2

                if (manager.IsJointTracked(userId2, i))
                {
                    Vector3 posJoint = manager.GetRawSkeletonJointPos(userId2, i);
                    

                    if (posJoint != Vector3.zero)
                    {
                        // convert the joint 3d position to depth 2d coordinates
                        Vector2 posDepth = manager.GetDepthMapPosForJointPos(posJoint);

                        float scaledX = posDepth.x * foregroundRect.width / KinectWrapper.Constants.DepthImageWidth;
                        float scaledY = posDepth.y * -foregroundRect.height / KinectWrapper.Constants.DepthImageHeight;

                        float screenX = foregroundOfs.x + scaledX;
                        float screenY = Camera.main.pixelHeight - (foregroundOfs.y + scaledY);
                        float zDistance = posJoint.z - Camera.main.transform.position.z;

                        Vector3 posScreen = new Vector3(screenX, screenY, zDistance);
                        Vector3 posCollider = Camera.main.ScreenToWorldPoint(posScreen);

                        jointColliders2[i].transform.position = posCollider;
                    }
                }

            }
        }

    }

    void OnGUI()
    {
        if (foregroundTex)
        {
            GUI.DrawTexture(foregroundRect, foregroundTex);
        }
    }

}
