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
        public Material TextureMappingMaterial;
        public List<GameObject> SampleObjects;

        public MiniatureRoom MiniatureRoom;

        public event Action SpatialMappingCreated = delegate { };
        //private bool scanComplete = false;

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

        protected void OnTextureUpdated()
        {
            StartCoroutine(UpdateAllMap());
        }

        protected IEnumerator UpdateAllMap()
        {
            var imageTextureMappingList = SpatialMapping.GetComponentsInChildren<ImageTextureMapping>();
            foreach (var imageTextureMapping in imageTextureMappingList)
            {
                imageTextureMapping.ApplyTextureMapping(TakePicture.Instance.worldToCameraMatrixList, TakePicture.Instance.projectionMatrixList, TakePicture.Instance.textureArray);
                yield return null;
            }

            foreach (var obj in SampleObjects)
            {
                if (obj.activeInHierarchy)
                {
                    imageTextureMappingList = obj.GetComponentsInChildren<ImageTextureMapping>();
                    foreach (var imageTextureMapping in imageTextureMappingList)
                    {
                        imageTextureMapping.ApplyTextureMapping(TakePicture.Instance.worldToCameraMatrixList, TakePicture.Instance.projectionMatrixList, TakePicture.Instance.textureArray);
                        yield return null;
                    }
                }
            }
        }

        void Start()
        {
            TakePicture.Instance.OnTextureUpdated += OnTextureUpdated;
        }

        public void StartTextureMapping()
        {
            SpatialUnderstanding.Instance.UnderstandingCustomMesh.MeshMaterial = TextureMappingMaterial;
            SpatialMappingManager.Instance.gameObject.SetActive(false);
        }

        private void SpatialMappingSource_SurfaceAdded(object sender, DataEventArgs<SpatialMappingSource.SurfaceObject> e)
        {
            ApplyTextureMapping(e.Data.Object);
            SpatialMappingCreated();
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
        }
    }
}