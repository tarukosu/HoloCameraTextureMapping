using HoloToolkit.Unity.SpatialMapping;
using UnityEngine;
using HoloToolkit.Unity;
using System.Collections.Generic;
using System;

namespace HoloCameraTextureMapping
{
    public class TextureMappingManager : Singleton<TextureMappingManager>
    {
        public GameObject SpatialMapping;
        public Texture2D SampleTexture;
        public Material TextureMappingMaterial;

        public Texture2DArray texture2DArray;
        public List<Matrix4x4> worldToCameraMatrixList = new List<Matrix4x4>();
        public List<Matrix4x4> projectionMatrixList = new List<Matrix4x4>();

        public List<GameObject> SampleObjects;


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
            //TODO
            //debug

            TakePicture.Instance.OnTextureUpdated += OnTextureUpdate;
        }

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