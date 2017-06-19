using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;
using HoloToolkit.Unity;
using System.Collections.Generic;
using System;
using System.Collections;

namespace HoloCameraTextureMapping
{
    public class TextureMappingManager : Singleton<TextureMappingManager>
    {
        public GameObject SpatialMapping;
        //public Texture2D SampleTexture;
        public Material TextureMappingMaterial;

        //public Texture2DArray texture2DArray;
        //public List<Matrix4x4> worldToCameraMatrixList = new List<Matrix4x4>();
        //public List<Matrix4x4> projectionMatrixList = new List<Matrix4x4>();

        public List<GameObject> SampleObjects;

        private bool scanComplete = false;


        private new void Awake()
        {
            base.Awake();
            var spatialMappingSources = SpatialMapping.GetComponents<SpatialMappingSource>();
            foreach (var source in spatialMappingSources)
            {
                source.SurfaceAdded += SpatialMappingSource_SurfaceAdded;
                source.SurfaceUpdated += SpatialMappingSource_SurfaceUpdated;
            }

        }

        private void OnTextureUpdate()
        {
            var imageTextureMappingList = SpatialMapping.GetComponentsInChildren<ImageTextureMapping>();
            foreach (var imageTextureMapping in imageTextureMappingList)
            {
                imageTextureMapping.ApplyTextureMapping(TakePicture.Instance.worldToCameraMatrixList, TakePicture.Instance.projectionMatrixList, TakePicture.Instance.textureArray);
            }

            foreach (var obj in SampleObjects)
            {
                imageTextureMappingList = obj.GetComponentsInChildren<ImageTextureMapping>();
                foreach (var imageTextureMapping in imageTextureMappingList)
                {
                    imageTextureMapping.ApplyTextureMapping(TakePicture.Instance.worldToCameraMatrixList, TakePicture.Instance.projectionMatrixList, TakePicture.Instance.textureArray);
                }
            }

        }

        void Start()
        {
            var spatialMappingManager = SpatialMappingManager.Instance;
            if (spatialMappingManager)
            {
                spatialMappingManager.SetSurfaceMaterial(TextureMappingMaterial);
            }

            // Use Spatial Understanding
            //SpatialUnderstanding.Instance.ScanStateChanged += Instance_ScanStateChanged;
            //SpatialUnderstanding.Instance.OnScanDone += Instance_ScanStateChanged;
            SpatialUnderstanding.Instance.RequestBeginScanning();

            TakePicture.Instance.OnTextureUpdated += OnTextureUpdate;

            StartCoroutine(FinishScanning());
            StartCoroutine(UpdateTexture());
        }
        private void Update()
        {
            if (scanComplete)
            {
                Debug.Log("scan completed!!");
                SpatialUnderstanding.Instance.RequestFinishScan();
                OnTextureUpdate();
                scanComplete = false;
            }
        }

        private IEnumerator FinishScanning()
        {
            yield return new WaitForSeconds(15);
            SpatialUnderstanding.Instance.RequestFinishScan();
            Debug.Log("finish!");
            OnTextureUpdate();
        }

        private IEnumerator UpdateTexture()
        {
            while (true)
            {
                OnTextureUpdate();
                yield return new WaitForSeconds(10);
            }
        }

        /*
        private void Instance_ScanStateChanged()
        {
            if ((SpatialUnderstanding.Instance.ScanState == SpatialUnderstanding.ScanStates.Done)
                && SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
            {
                scanComplete = true;
            }
        }
        */


        private void SpatialMappingSource_SurfaceAdded(object sender, DataEventArgs<SpatialMappingSource.SurfaceObject> e)
        {
            ApplyTextureMapping(e.Data.Object);
        }

        private void SpatialMappingSource_SurfaceUpdated(object sender, DataEventArgs<SpatialMappingSource.SurfaceUpdate> e)
        {
            ApplyTextureMapping(e.Data.New.Object);
        }

        private void ApplyTextureMapping(GameObject obj)
        {

            var imageTextureMapping = obj.GetComponent<ImageTextureMapping>();
            if (imageTextureMapping == null)
            {
                imageTextureMapping = obj.AddComponent<ImageTextureMapping>();
            }

            imageTextureMapping.ApplyTextureMapping(TakePicture.Instance.worldToCameraMatrixList, TakePicture.Instance.projectionMatrixList, TakePicture.Instance.textureArray);
            //imageTextureMapping.SampleTexture = SampleTexture;
            //imageTextureMapping.ApplyTextureMapping(TakePicture.Instance.worldToCameraMatrixArray, TakePicture.Instance.projectionMatrixArray, TakePicture.Instance.textureArray);
        }
    }
}