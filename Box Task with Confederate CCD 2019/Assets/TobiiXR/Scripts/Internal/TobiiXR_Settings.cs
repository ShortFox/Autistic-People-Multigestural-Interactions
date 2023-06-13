﻿// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR
{
    using UnityEngine;
    using G2OM;
    using System;

    public class TobiiXR_Settings : ScriptableObject
    {
        public static readonly string TobiiXR_DescriptionPath = typeof(TobiiXR_Settings).Name;
        private static RuntimePlatform _platform = Application.platform;
        private IEyeTrackingProvider _eyeTrackingProvider;
        private G2OM _g2om;

        public static Func<TobiiXR_Settings> LoadDefaultSettings = () => Resources.Load<TobiiXR_Settings>(TobiiXR_DescriptionPath);

        public IEyeTrackingProvider EyeTrackingProvider
        {
            get
            {
                if (_eyeTrackingProvider != null) return _eyeTrackingProvider;
                _eyeTrackingProvider = GetProvider();
                return _eyeTrackingProvider;
            }
            set
            {
                _eyeTrackingProvider = value;
            }
        }

        public G2OM G2OM
        {
            get
            {
                if (_g2om != null) return _g2om;

                _g2om = G2OM.Create(new G2OM_Description
                {
                    LayerMask = LayerMask,
                    ExpectedNumberOfObjects = ExpectedNumberOfObjects,
                    HowLongToKeepCandidatesInSeconds = HowLongToKeepCandidatesInSeconds
                });

                return _g2om;
            }
            set { _g2om = value; }
        }

        [HideInInspector]
        public string EyeTrackingProviderTypeStandAlone = typeof(NoseDirectionProvider).FullName;

        [HideInInspector]
        public string EyeTrackingProviderTypeAndroid = typeof(NoseDirectionProvider).FullName;

        [HideInInspector]
        public LayerMask LayerMask = G2OM_Description.DefaultLayerMask;

        [HideInInspector]
        public int ExpectedNumberOfObjects = G2OM_Description.DefaultExpectedNumberOfObjects;

        [HideInInspector]
        public float HowLongToKeepCandidatesInSeconds = G2OM_Description.DefaultCandidateMemoryInSeconds;

        public static TobiiXR_Settings CreateDefaultSettings()
        {
            bool b;
            return CreateDefaultSettings(out b);
        }

        public static TobiiXR_Settings CreateDefaultSettings(out bool resourceExists)
        {
            var configuration = LoadDefaultSettings != null ? LoadDefaultSettings() : null;
            resourceExists = configuration != null;
            return resourceExists ? configuration : ScriptableObject.CreateInstance<TobiiXR_Settings>();
        }

        public Type GetProviderType()
        {
            string eyeTrackingProviderType = _platform == RuntimePlatform.Android
                ? EyeTrackingProviderTypeAndroid
                : EyeTrackingProviderTypeStandAlone;

            if (string.IsNullOrEmpty(eyeTrackingProviderType)) return null;
            return Type.GetType(eyeTrackingProviderType);
        }

        private IEyeTrackingProvider GetProvider()
        {
            var type = GetProviderType();
            if (type == null) return null;
            try
            {
                return Activator.CreateInstance(type) as IEyeTrackingProvider;
            }
            catch (Exception) { }
            return null;
        }
    }
}