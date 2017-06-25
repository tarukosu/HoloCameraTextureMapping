using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HoloCameraTextureMapping {
    public class MiniatureRoom : MonoBehaviour {
        public Dictionary<string, GameObject> MiniatureMeshDictionary = new Dictionary<string, GameObject>();

        void Start() {
            TakePicture.Instance.OnTextureUpdated += OnTextureUpdated;
        }

        void Update() {
            
        }

        protected void OnTextureUpdated()
        {
            GenerateMesh();
        }

        public void GenerateMesh()
        {
            var imageTextureMappingList = SpatialUnderstanding.Instance.gameObject.transform;
            foreach (Transform imageTextureMapping in imageTextureMappingList)
            {
                var name = imageTextureMapping.name;
                if (MiniatureMeshDictionary.ContainsKey(name))
                {

                    var meshObject = MiniatureMeshDictionary[name];
                    meshObject.GetComponent<Renderer>().material = imageTextureMapping.gameObject.GetComponent<Renderer>().material;
                }
                else
                {
                    var meshObject = Instantiate(imageTextureMapping.gameObject, transform);
                    meshObject.transform.localScale = 0.1f * Vector3.one;
                    MiniatureMeshDictionary.Add(name, meshObject);
                }
            }
        }
    }
}