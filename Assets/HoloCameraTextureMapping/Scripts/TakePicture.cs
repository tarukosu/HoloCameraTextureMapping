/*
 * Copyright 2017 tarukosu
 * This file contains Original Code and/or Modifications of Original Code
 * licensed under the Apache License, Version 2.0 (the "License");
 * Orignal Code is
 *  Copyright 2017 ywj7931
 *  https://github.com/ywj7931/Hololens_Image_Based_Texture/blob/master/Assets/Scripts/TakePicture.cs
 */
 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VR.WSA.WebCam;
using UnityEngine.VR.WSA.Input;
using System.IO;
using HoloToolkit.Unity;
using HoloCameraTextureMapping;
using HoloToolkit.Unity.SpatialMapping;
using System;

//Imports to interact with Camera
#if WINDOWS_UWP
using System.Runtime.InteropServices;
using Windows.Media.Devices;
#endif

public class TakePicture : Singleton<TakePicture>
{
    //data copying fronm list to pass to shader
    public Matrix4x4[] worldToCameraMatrixArray;
    public Matrix4x4[] projectionMatrixArray;
    public Texture2DArray textureArray;

    //the max photos numbers that can be taken, shader doesn't support dynamic array
    public int maxPhotoNum;
    //Locks in the exposure and white balance after first photo.
    public bool lockCameraSettings = true;
    Texture2D destTex;

    //camera paramaters
    private CameraParameters m_CameraParameters;
    Texture2D m_Texture;
    byte[] bytes;

    //temp data waiting to pass to shader
    //private List<Texture2D> textureList;
    public List<Matrix4x4> projectionMatrixList;
    public List<Matrix4x4> worldToCameraMatrixList;


    private bool isCapturingPhoto = false;
    private PhotoCapture photoCaptureObj;
    private int currentPhoto = 0;
    private GestureRecognizer recognizer;

    public GameObject SpatialMapping;
    private ImageTextureMapping[] imageTextureMappingList;
#if UNITY_EDITOR
    private const int TEXTURE_WIDTH = 512;
    private const int TEXTURE_HEIGHT = 256;
#else
    private const int TEXTURE_WIDTH = 1024;//512;
    private const int TEXTURE_HEIGHT = 512;//256
#endif

    public event Action OnTextureUpdated;

#region debug
    public Texture2D[] SampleTexture;
#endregion
    // Use this for initialization
    void Start()
    {
        //init
        //textureList = new List<Texture2D>();
        projectionMatrixList = new List<Matrix4x4>();
        worldToCameraMatrixArray = new Matrix4x4[maxPhotoNum];
        projectionMatrixArray = new Matrix4x4[maxPhotoNum];
        worldToCameraMatrixList = new List<Matrix4x4>();

        //init camera
        InitCamera();

        imageTextureMappingList = SpatialMapping.GetComponentsInChildren<ImageTextureMapping>();

        //for debug
        //OnPhotoCapturedDebug();
#if UNITY_EDITOR
        StartCoroutine(DebugCapture());
#endif
    }

    IEnumerator DebugCapture()
    {
        while (true)
        {
            yield return new WaitForSeconds(4);
            OnPhotoCapturedDebug();
        }
    }


    void InitCamera()
    {
        List<Resolution> resolutions = new List<Resolution>(PhotoCapture.SupportedResolutions);

        //1280 * 720
        Resolution selectedResolution = resolutions[0];

        //camera params
        m_CameraParameters = new CameraParameters(WebCamMode.PhotoMode)
        {
            cameraResolutionWidth = selectedResolution.width,
            cameraResolutionHeight = selectedResolution.height,
            hologramOpacity = 0.0f,
            pixelFormat = CapturePixelFormat.BGRA32
        };

        textureArray = new Texture2DArray(TEXTURE_WIDTH, TEXTURE_HEIGHT, maxPhotoNum, TextureFormat.DXT5, false);
        var clearTexture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.ARGB32, false);
        //Color resetColor = Color.clear;
        var resetColorArray = clearTexture.GetPixels();
        for (int i = 0; i < resetColorArray.Length; i++)
        {
            resetColorArray[i] = Color.clear;
        }
        clearTexture.SetPixels(resetColorArray);
        clearTexture.Apply();
        clearTexture.Compress(true);

        Graphics.CopyTexture(clearTexture, 0, 0, textureArray, 0, 0); //Copies the last texture
        //   m_Texture = new Texture2D(m_CameraParameters.cameraResolutionWidth, m_CameraParameters.cameraResolutionHeight, TextureFormat.ARGB32, false);
        //init photocaptureobject
        PhotoCapture.CreateAsync(false, OnCreatedPhotoCaptureObject);

        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            OnPhotoKeyWordDetected();
        };
        recognizer.StartCapturingGestures();
    }

    void OnCreatedPhotoCaptureObject(PhotoCapture captureObject)
    {

        photoCaptureObj = captureObject;

        //photoCaptureObj.StartPhotoModeAsync(m_CameraParameters, true, OnStartPhotoMode);
        photoCaptureObj.StartPhotoModeAsync(m_CameraParameters, OnStartPhotoMode);
        //TODO Dispose PhotocaptureObj on cleanup
    }

    void OnStartPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        //TextManager.Instance.setText("Ready to take photos, say \"Photo\"");
    }

    public void OnPhotoKeyWordDetected()
    {
        //if it is capturing photo now, just return
        if (isCapturingPhoto)
        {
            return;
        }

        isCapturingPhoto = true;
        //TextManager.Instance.setText("Taking picture...");

        photoCaptureObj.TakePhotoAsync(OnPhotoCaptured);
    }

    void OnPhotoCaptured(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        //After the first photo, we want to lock in the current exposure and white balance settings.
        if (lockCameraSettings && currentPhoto == 1)
        {
#if WINDOWS_UWP
        unsafe{
            //This is how you access a COM object.
            VideoDeviceController vdm = (VideoDeviceController)Marshal.GetObjectForIUnknown(photoCaptureObj.GetUnsafePointerToVideoDeviceController());
            //Locks current exposure for all future images
            vdm.ExposureControl.SetAutoAsync(false); //Figureout how to better handle the Async
            //Locks the current WhiteBalance for all future images
            vdm.WhiteBalanceControl.SetPresetAsync(ColorTemperaturePreset.Manual); 
        }
#endif
        }
        //get this renderer
        Renderer m_CanvasRenderer = GetComponent<Renderer>() as Renderer;
        // m_CanvasRenderer.material = new Material(Shader.Find("Unlit/ColorRoomShader"));
        //temp to store the matrix
        Matrix4x4 cameraToWorldMatrix;
        photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
        Matrix4x4 worldToCameraMatrix = cameraToWorldMatrix.inverse;

        Matrix4x4 projectionMatrix;
        photoCaptureFrame.TryGetProjectionMatrix(out projectionMatrix);

        //add to matrixList

        //for debug
        Debug.Log("hogehoge");
        Debug.Log(projectionMatrix);
        Debug.Log(projectionMatrix.m00);
        Debug.Log(projectionMatrix.m01);
        Debug.Log(projectionMatrix.m02);
        Debug.Log(projectionMatrix.m03);

        //projectionMatrix.m00 = 320;
#if UNITY_EDITOR
        projectionMatrix.m00 = 2.43241f;
        projectionMatrix.m02 = 0.07031f;
        projectionMatrix.m11 = 4.31949f;
        projectionMatrix.m12 = 0.02752f;
        projectionMatrix.m22 = -1f;
        projectionMatrix.m32 = -1f;
        projectionMatrix.m33 = 0f;
        Debug.Log(projectionMatrix);
        /*
        2.43241 0.00000 0.07031 0.00000
        0.00000 4.31949 0.02752 0.00000
        0.00000 0.00000 - 1.00000    0.00000
        0.00000 0.00000 - 1.00000    0.00000
        */
#endif


        /*
        projectionMatrixList.Clear();
        worldToCameraMatrixList.Clear();
        currentPhoto = 0;
        */
        //for debug

        projectionMatrixList.Add(projectionMatrix);
        worldToCameraMatrixList.Add(worldToCameraMatrix);


        m_Texture = new Texture2D(m_CameraParameters.cameraResolutionWidth, m_CameraParameters.cameraResolutionHeight, TextureFormat.RGBA32, false);
        photoCaptureFrame.UploadImageDataToTexture(m_Texture);

        m_Texture.wrapMode = TextureWrapMode.Clamp;

        //m_Texture = ResizeTexture(m_Texture, 1024, 512);
        m_Texture = ResizeTexture(m_Texture, TEXTURE_WIDTH, TEXTURE_HEIGHT);
        //TODO 

        //textureList.Add(m_Texture);
        photoCaptureFrame.Dispose();

        //save room to png
        bytes = m_Texture.EncodeToPNG();
        //write to LocalState folder
        File.WriteAllBytes(Application.persistentDataPath + "/Room" + (currentPhoto + 1) + ".png", bytes);


        //update matrix and texturearray in shader
        //Graphics.CopyTexture(textureList[textureList.Count - 1], 0, 0, array, textureList.Count - 1, 0);
        m_Texture.Compress(true);
        Debug.Log(m_Texture.format);
        //debug
        Graphics.CopyTexture(m_Texture, 0, 0, textureArray, currentPhoto + 1, 0); //Copies the last texture
        //
        worldToCameraMatrixArray[currentPhoto] = worldToCameraMatrixList[currentPhoto];
        projectionMatrixArray[currentPhoto] = projectionMatrixList[currentPhoto];

        if (OnTextureUpdated != null)
        {
            OnTextureUpdated();
        }
        /*
        imageTextureMappingList = SpatialMapping.GetComponentsInChildren<ImageTextureMapping>();
        foreach (var imageTextureMapping in imageTextureMappingList)
        {

            imageTextureMapping.AddTextureMapping(worldToCameraMatrix, projectionMatrix, m_Texture);
            //imageTextureMapping.UpdateTextureMapping(worldToCameraMatrix, projectionMatrix, m_Texture);
            //imageTextureMapping.UpdateTextureMapping();
            }
        */
        //set shader properties

        /*
        GetComponent<Renderer>().sharedMaterial.SetTexture("_MyArr", array);
        GetComponent<Renderer>().sharedMaterial.SetMatrixArray("_WorldToCameraMatrixArray", worldToCameraMatrixArray);
        GetComponent<Renderer>().sharedMaterial.SetMatrixArray("_CameraProjectionMatrixArray", projectionMatrixArray);
        */
        //TextManager.Instance.setText(currentPhoto + 1 + " Photos Taken");
        currentPhoto += 1; //Increments the counter

        isCapturingPhoto = false;

        Resources.UnloadUnusedAssets();
    }


    void OnPhotoCapturedDebug()
    {
        /*
        if (currentPhoto == 1)
        {
            return;
        }
        */


        if (currentPhoto == SampleTexture.Length)
        {
            return;
        }
        //get this renderer
        Renderer m_CanvasRenderer = GetComponent<Renderer>() as Renderer;
        // m_CanvasRenderer.material = new Material(Shader.Find("Unlit/ColorRoomShader"));
        //temp to store the matrix
        Matrix4x4 cameraToWorldMatrix = Matrix4x4.identity;
        cameraToWorldMatrix.m00 = -1f;
        cameraToWorldMatrix.m11 = -1f;
        cameraToWorldMatrix.m22 = -1f;
        
        if(currentPhoto % SampleTexture.Length == 1)
        {
            cameraToWorldMatrix.m03 = 0.3f;
        }

        //photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
        Matrix4x4 worldToCameraMatrix = cameraToWorldMatrix.inverse;

        Matrix4x4 projectionMatrix = Matrix4x4.identity;
        projectionMatrix.m00 = 2.43241f;
        projectionMatrix.m02 = 0.07031f;
        projectionMatrix.m11 = 4.31949f;
        projectionMatrix.m12 = 0.02752f;
        projectionMatrix.m22 = -1f;
        projectionMatrix.m32 = -1f;
        projectionMatrix.m33 = 0f;
        Debug.Log(projectionMatrix);
        /*
        2.43241 0.00000 0.07031 0.00000
        0.00000 4.31949 0.02752 0.00000
        0.00000 0.00000 - 1.00000    0.00000
        0.00000 0.00000 - 1.00000    0.00000
        */
        //for debug

        /*
        projectionMatrixList.Clear();
        worldToCameraMatrixList.Clear();
        currentPhoto = 0;
        */
        //for debug

        projectionMatrixList.Add(projectionMatrix);
        worldToCameraMatrixList.Add(worldToCameraMatrix);

        /*
        if (currentPhoto == SampleTexture.Length - 1)
        {
            return;
        }
        */


        var texture = SampleTexture[currentPhoto % SampleTexture.Length];
        Debug.Log(texture.format);


        //SampleTexture = ResizeTexture(SampleTexture, TEXTURE_WIDTH, TEXTURE_HEIGHT);
        //texture = ResizeTexture(texture, TEXTURE_WIDTH, TEXTURE_HEIGHT);

        //textureArray = new Texture2DArray(SampleTexture.width, SampleTexture.height, 1, TextureFormat.DXT5, false);
        //Graphics.CopyTexture(SampleTexture, 0, 0, textureArray, 0, 0);
        Graphics.CopyTexture(texture, 0, 0, textureArray, currentPhoto + 1, 0);

        /*
        //m_Texture = new Texture2D(m_CameraParameters.cameraResolutionWidth, m_CameraParameters.cameraResolutionHeight, TextureFormat.RGBA32, false);
        //photoCaptureFrame.UploadImageDataToTexture(m_Texture);

        m_Texture.wrapMode = TextureWrapMode.Clamp;

        //m_Texture = ResizeTexture(m_Texture, 1024, 512);
        //m_Texture = ResizeTexture(m_Texture, TEXTURE_WIDTH, TEXTURE_HEIGHT);
        //TODO 

        //save room to png
        bytes = m_Texture.EncodeToPNG();

        //update matrix and texturearray in shader
        //Graphics.CopyTexture(textureList[textureList.Count - 1], 0, 0, array, textureList.Count - 1, 0);
        m_Texture.Compress(true);
        Debug.Log(m_Texture.format);
        Graphics.CopyTexture(m_Texture, 0, 0, textureArray, currentPhoto, 0); //Copies the last texture
        worldToCameraMatrixArray[currentPhoto] = worldToCameraMatrixList[currentPhoto];
        projectionMatrixArray[currentPhoto] = projectionMatrixList[currentPhoto];
        */
        if (OnTextureUpdated != null)
        {
            OnTextureUpdated();
        }
        currentPhoto += 1; //Increments the counter
        isCapturingPhoto = false;
        //Resources.UnloadUnusedAssets();
    }

    //Helper function to resize from top left
    Texture2D ResizeTexture(Texture2D input, int width, int height)
    {
        Color[] pix = input.GetPixels(0, input.height - height, width, height);
        input.Resize(width, height);
        input.SetPixels(pix);
        input.Apply();
        return input;
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObj.Dispose();
        photoCaptureObj = null;
    }

    public void StopCamera()
    {
        if (photoCaptureObj != null)
        {
            photoCaptureObj.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        //TextManager.Instance.setText("");
    }

}
