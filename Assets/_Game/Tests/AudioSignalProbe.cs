#if UNITY_EDITOR
using System;
using System.IO;
using EchoProtocol.Presentation;
using UnityEngine;

public static class AudioSignalProbe
{
    public static void Run()
    {
        var g=new GameObject("Temporary audio signal test");g.SetActive(false);
        try
        {
            var output=g.AddComponent<SessionAudio>();
            float[] stereo={.9f,-.9f};output.ProcessSamples(stereo,2);
            Require(Mathf.Abs(stereo[0]-.65f)<.0001f&&Mathf.Abs(stereo[1]+.65f)<.0001f,"normal linked-channel bound");
            output.ReducedDynamics=true;stereo=new[]{.9f,-.9f};output.ProcessSamples(stereo,2);
            Require(Mathf.Abs(stereo[0]-.18f)<.0001f,"reduced impact bound");
            output.Speakers=true;stereo=new[]{.6f,.2f};output.ProcessSamples(stereo,2);
            Require(stereo[0]==stereo[1],"mono output channels agree");
            output.Speakers=false;output.ReducedDynamics=false;
            float[] quiet={.02f,.03f};output.ProcessSamples(quiet,2);
            Require(quiet[0]<=.02f&&quiet[1]<=.03f,"quiet signal never amplified");
            File.WriteAllText("Docs/VisualEvidence/2026-09-30-CompleteEdition/audio-signal-checks.txt","PASS normal stereo bound 0.65\nPASS reduced bound 0.18\nPASS linked-channel processing\nPASS mono agreement\nPASS no upward gain\nThese checks exercise the actual sample processor; they do not measure device loudness or human comfort.\n");
            Debug.Log("AUDIO SIGNAL CHECKS PASS");
        }
        finally { UnityEngine.Object.DestroyImmediate(g); }
    }
    private static void Require(bool ok,string name){if(!ok)throw new Exception("Audio test failed: "+name);}
}
#endif
