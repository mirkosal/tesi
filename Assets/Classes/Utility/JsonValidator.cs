using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class JsonValidator
{
    public static List<HandData> ValidateAndTransformJson(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            UnityEngine.Debug.Log("JSON data is null or empty.");
            return new List<HandData>();
        }

        if (!IsValidJson(jsonData))
        {
            UnityEngine.Debug.Log("Invalid JSON data.");
            return new List<HandData>();
        }

        JArray jsonArray = JArray.Parse(jsonData);
        List<HandData> validHandDataList = new List<HandData>();

        foreach (var item in jsonArray)
        {
            HandData handData = new HandData();
            handData.DeviceID = item.Value<int?>("DeviceID") ?? 0;
            handData.Id = item.Value<int?>("Id") ?? 0;
            handData.Timestamp = item.Value<long?>("Timestamp") ?? 0;
            handData.CurrentFramesPerSecond = item.Value<float?>("CurrentFramesPerSecond") ?? 0f;
            handData.ActivityID = item.Value<int?>("ActivityID") ?? 0;
            handData.id0 = item.Value<string>("id0") ?? string.Empty;

            JArray handsArray = item.Value<JArray>("Hands");
            if (handsArray != null)
            {
                foreach (var handItem in handsArray)
                {
                    Hand hand = new Hand();
                    hand.Id = handItem.Value<int?>("Id") ?? 0;
                    hand.FrameId = handItem.Value<int?>("FrameId") ?? 0;
                    hand.DeviceID = handItem.Value<int?>("DeviceID") ?? 0;
                    hand.HandDataID = handItem.Value<int?>("HandDataID") ?? 0;
                    hand.PalmPosition = ParseVector3(handItem["PalmPosition"]);
                    hand.PalmVelocity = ParseVector3(handItem["PalmVelocity"]);
                    hand.PalmNormal = ParseVector3(handItem["PalmNormal"]);
                    hand.Direction = ParseVector3(handItem["Direction"]);
                    hand.Rotation = ParseQuaternion(handItem["Rotation"]);
                    hand.GrabStrength = handItem.Value<float?>("GrabStrength") ?? 0f;
                    hand.PinchStrength = handItem.Value<float?>("PinchStrength") ?? 0f;
                    hand.PinchDistance = handItem.Value<float?>("PinchDistance") ?? 0f;
                    hand.IsExtended = handItem.Value<bool?>("IsExtended") ?? false;
                    hand.TimeVisible = handItem.Value<double?>("TimeVisible") ?? 0.0;

                    JArray fingersArray = handItem.Value<JArray>("Fingers");
                    if (fingersArray != null)
                    {
                        foreach (var fingerItem in fingersArray)
                        {
                            Finger finger = new Finger();
                            finger.Id = fingerItem.Value<int?>("Id") ?? 0;
                            finger.Type = fingerItem.Value<int?>("Type") ?? 0;
                            finger.HandId = fingerItem.Value<int?>("HandId") ?? 0;
                            finger.TipPosition = ParseVector3(fingerItem["TipPosition"]);
                            finger.Direction = ParseVector3(fingerItem["Direction"]);
                            finger.Width = fingerItem.Value<float?>("Width") ?? 0f;
                            finger.Length = fingerItem.Value<float?>("Length") ?? 0f;
                            finger.IsExtended = fingerItem.Value<bool?>("IsExtended") ?? false;
                            finger.TimeVisible = fingerItem.Value<double?>("TimeVisible") ?? 0.0;

                            JArray bonesArray = fingerItem.Value<JArray>("bones");
                            if (bonesArray != null)
                            {
                                foreach (var boneItem in bonesArray)
                                {
                                    Bone bone = new Bone();
                                    bone.BoneID = boneItem.Value<int?>("BoneID") ?? 0;
                                    bone.PrevJoint = ParseVector3(boneItem["PrevJoint"]);
                                    bone.NextJoint = ParseVector3(boneItem["NextJoint"]);
                                    bone.Center = ParseVector3(boneItem["Center"]);
                                    bone.Rotation = ParseQuaternion(boneItem["Rotation"]);
                                    bone.Length = boneItem.Value<float?>("Length") ?? 0f;
                                    bone.Width = boneItem.Value<float?>("Width") ?? 0f;
                                    bone.Type = boneItem.Value<int?>("Type") ?? 0;
                                    bone.FingerID = boneItem.Value<int?>("FingerID") ?? 0;
                                    finger.bones.Add(bone);
                                }
                            }

                            hand.Fingers.Add(finger);
                        }
                    }

                    handData.Hands.Add(hand);
                }
            }

            validHandDataList.Add(handData);
        }

        return validHandDataList;
    }

    private static bool IsValidJson(string jsonData)
    {
        try
        {
            JToken.Parse(jsonData);
            return true;
        }
        catch (JsonReaderException ex)
        {
            UnityEngine.Debug.Log($"JSON parsing error at line {ex.LineNumber}, position {ex.LinePosition}: {ex.Message}");
            return false;
        }
    }

    private static Vector3 ParseVector3(JToken token)
    {
        if (token == null) return Vector3.zero;
        return new Vector3(
            token.Value<float?>("x") ?? 0f,
            token.Value<float?>("y") ?? 0f,
            token.Value<float?>("z") ?? 0f
        );
    }

    private static Quaternion ParseQuaternion(JToken token)
    {
        if (token == null) return Quaternion.identity;
        return new Quaternion(
            token.Value<float?>("x") ?? 0f,
            token.Value<float?>("y") ?? 0f,
            token.Value<float?>("z") ?? 0f,
            token.Value<float?>("w") ?? 1f
        );
    }
}
