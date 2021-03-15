using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO.Ports;
using System;
using FYDP.ArmBrace;
using FYDP.VR;
using FYDP.Utils;
using UnityEditor;

public class Sandbox : MonoBehaviour
{
    MotorCmdFormat mcf;
    void Start(){
        mcf = new MotorCmdFormat(
                    torqueRatingNm:1.2f, torqueCmdFullScale:89, gearRatio:5, 
                    stictionEncodedTorque:8);
    }
    void Update(){
        mcf.SetTorqueMove(-100.098098900f);
        Debug.Log("MOVE: " + mcf.Data.ToString());
        mcf.SetTorqueHold(-100.098098900f);
        Debug.Log("MOVE: " + mcf.Data.ToString());
    }
}