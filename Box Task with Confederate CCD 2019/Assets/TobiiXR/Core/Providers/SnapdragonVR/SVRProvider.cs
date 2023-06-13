// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEditor;

namespace Tobii.XR
{
    using UnityEngine;

    [CompilerFlag("TOBIIXR_SNAPDRAGONVRPROVIDER"), SupportedPlatform(XRBuildTargetGroup.Android)]
    public class SnapdragonVRProvider : IEyeTrackingProvider
    {
        public TobiiXR_EyeTrackingData EyeTrackingData { get; private set; }

        public SnapdragonVRProvider()
        {
            #if TOBIIXR_SNAPDRAGONVRPROVIDER
            SvrManager.Instance.settings.trackEyes = true;
            EyeTrackingData = new TobiiXR_EyeTrackingData();
            #endif
        }

        public void Tick()
        {
            #if TOBIIXR_SNAPDRAGONVRPROVIDER
            var eyePose = SvrManager.Instance.EyePose;
            var head = SvrManager.Instance.head;

            EyeTrackingData.Timestamp = Time.unscaledTime;

            EyeTrackingData.Left.Ray = new TobiiXR_GazeRay
            {
                Direction = head.TransformDirection(eyePose.leftDirection),
                Origin = head.TransformPoint(eyePose.leftPosition),
                IsValid = (eyePose.leftStatus & (int)SvrPlugin.EyePoseStatus.kGazePointValid) != 0 && (eyePose.leftStatus & (int)SvrPlugin.EyePoseStatus.kGazeVectorValid) != 0,
            };

            EyeTrackingData.Right.Ray = new TobiiXR_GazeRay
            {
                Direction = head.TransformDirection(eyePose.rightDirection),
                Origin = head.TransformPoint(eyePose.rightPosition),
                IsValid = (eyePose.rightStatus & (int)SvrPlugin.EyePoseStatus.kGazePointValid) != 0 && (eyePose.rightStatus & (int)SvrPlugin.EyePoseStatus.kGazeVectorValid) != 0
            };

            EyeTrackingData.CombinedRay = new TobiiXR_GazeRay
            {
                Direction = EyeTrackingData.Left.Ray.Direction + EyeTrackingData.Left.Ray.Direction,
                Origin = (EyeTrackingData.Left.Ray.Origin + EyeTrackingData.Right.Ray.Origin) / 2f,
                IsValid = EyeTrackingData.Left.Ray.IsValid && EyeTrackingData.Right.Ray.IsValid
            };

            EyeTrackingData.Left.EyeOpenness = eyePose.leftOpenness;
            EyeTrackingData.Left.EyeOpennessIsValid = (eyePose.leftStatus & (int)SvrPlugin.EyePoseStatus.kEyeOpennessValid) != 0;
            EyeTrackingData.Right.EyeOpenness = eyePose.rightOpenness;
            EyeTrackingData.Right.EyeOpennessIsValid = (eyePose.rightStatus & (int)SvrPlugin.EyePoseStatus.kEyeOpennessValid) != 0;
            #endif
        }

        public void Destroy()
        {
            // TODO: Should SVRManager be destroyed here?
        }
    }
}