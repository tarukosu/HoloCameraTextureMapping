using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HUX.Dialogs;
using HUX.Interaction;
using HoloToolkit.Unity.SpatialMapping;

namespace HoloCameraTextureMapping {
    public class AppManager : Singleton<AppManager> {
        public enum AppStates
        {
            Loading, Scanning, MappingTexture
        };

        public AppStates State { get; protected set; }
        public GameObject DialogPrefab;
        public MiniatureRoom MiniatureRoom;

        protected SimpleDialog dialog;
        //protected SimpleDialog finishScanningdialog;

        void Start() {
            State = AppStates.Loading;
            StartCoroutine(DisplayLoadingMessage());
            //StartCoroutine(DisplayScanningMessage());
            TextureMappingManager.Instance.SpatialMappingCreated += SpatialMappingCreated;
            InteractionManager.OnTapped += TappedCallBack;
        }

        private void TappedCallBack(GameObject obj, InteractionManager.InteractionEventArgs arg)
        {
            if(obj == null ||
                (obj.transform.parent != null && obj.transform.parent.gameObject == SpatialMappingManager.Instance.gameObject) ||
                (obj.transform.parent != null && obj.transform.parent.gameObject == SpatialUnderstanding.Instance.gameObject))
            {
                //Debug.Log(obj.transform.parent.gameObject.ToString());
                switch (State)
                {
                    case AppStates.Scanning:
                        DisplayFinishScanningDialog();
                        break;
                    case AppStates.MappingTexture:
                        Debug.Log("Take Photo");
                        TakePicture.Instance.TakePhoto();
                        break;
                }
            }
        }

        void Update()
        {

        }

        protected void SpatialMappingCreated()
        {
            if (State == AppStates.Loading)
            {
                State = AppStates.Scanning;
                Debug.Log("Start Scanning");
                StartCoroutine(DisplayScanningMessage());
            }
        }

        protected IEnumerator DisplayLoadingMessage()
        {
            var message = "Mapping your surroundings...";

            LoadingDialog.Instance.Open(
                                LoadingDialog.IndicatorStyleEnum.AnimatedOrbs,
                                LoadingDialog.ProgressStyleEnum.None,
                                LoadingDialog.MessageStyleEnum.Visible,
                                message);
            while (State == AppStates.Loading)
            {
                yield return null;
            }
            LoadingDialog.Instance.Close();
            while (LoadingDialog.Instance.IsLoading)
            {
                yield return null;
            }
        }

        protected IEnumerator DisplayScanningMessage()
        {
            yield return new WaitForSeconds(1);

            ClearDialog();
            SimpleDialog.ButtonTypeEnum buttons = SimpleDialog.ButtonTypeEnum.OK;
            dialog = SimpleDialog.Open(DialogPrefab, buttons, "Creating Spatial Mapping", "Look around your surroundings.\nTo stop spatial mapping, do AirTap.");
            dialog.OnClosed += OnScanningDialogClosed;

            //debug
            //yield return new WaitForSeconds(5);
            //DisplayFinishScanningDialog();
        }

        protected void OnScanningDialogClosed(SimpleDialogResult result)
        {
            Debug.Log(result.Result);
        }

        protected void DisplayFinishScanningDialog()
        {
            ClearDialog();
            SimpleDialog.ButtonTypeEnum buttons = SimpleDialog.ButtonTypeEnum.Yes | SimpleDialog.ButtonTypeEnum.No;
            dialog = SimpleDialog.Open(DialogPrefab, buttons, "Stop Updating Spatial Mapping", "Are you sure you want to stop updating spatial mapping?");
            dialog.OnClosed += OnFinishScanningDialogClosed;
        }

        protected void OnFinishScanningDialogClosed(SimpleDialogResult result)
        {
            Debug.Log(result.Result);
            if(result.Result == SimpleDialog.ButtonTypeEnum.Yes)
            {
                StartCoroutine(StartMappingTexture());
            }
        }


        public IEnumerator StartMappingTexture()
        {
            if (State == AppStates.Scanning)
            {
                State = AppStates.MappingTexture;

                SpatialUnderstanding.Instance.RequestFinishScan();
                while(SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Done)
                {
                    yield return null;

                }
                //                SpatialUnderstanding.Instance.UnderstandingCustomMesh.MeshMaterial = TextureMappingManager.Instance.TextureMappingMaterial;
                TextureMappingManager.Instance.StartTextureMapping();
                MiniatureRoom.GenerateMesh();

                ClearDialog();
                SimpleDialog.ButtonTypeEnum buttons = SimpleDialog.ButtonTypeEnum.OK;
                dialog = SimpleDialog.Open(DialogPrefab, buttons, "Camera Mapping", "Move around and do AirTap.");
            }
        }

        protected void ClearDialog()
        {
            if (dialog != null && dialog.gameObject != null)
            {
                Destroy(dialog.gameObject);
            }

        }

    }
}