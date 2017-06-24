using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HUX.Dialogs;

namespace HoloCameraTextureMapping {
    public class AppManager : Singleton<AppManager> {
        public enum AppStates
        {
            Loading, Scanning, MappingTexture
        };

        public AppStates State { get; protected set; }
        public GameObject DialogPrefab;

        void Start() {
            State = AppStates.Loading;
            StartCoroutine(DisplayLoadingMessage());
            TextureMappingManager.Instance.SpatialMappingCreated += SpatialMappingCreated;
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
            //StartCoroutine(LoadOverTime(LoadTextMessage));

            /*
            yield return new WaitForSeconds(1);
            SimpleDialogResult result = new SimpleDialogResult();
            SimpleDialog.ButtonTypeEnum buttons = SimpleDialog.ButtonTypeEnum.None;
            SimpleDialog dialog = SimpleDialog.Open(DialogPrefab, buttons, "aa","aa");
            yield return null;
            */
        }

        public void StartMappingTexture()
        {
            if (State == AppStates.Scanning)
            {
                SpatialUnderstanding.Instance.RequestFinishScan();
                State = AppStates.MappingTexture;
            }
        }

    }
}