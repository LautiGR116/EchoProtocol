#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using EchoProtocol.Campaign;
using EchoProtocol.Echo;
using EchoProtocol.Player;
using EchoProtocol.Presentation;

public sealed class CampaignCheckpointProbe:MonoBehaviour
{
    const string Key="EP.CampaignCheckpoint.v1";
    string Out=>"Docs/VisualEvidence/2026-09-30-"+(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Finish")?"Finish":"Campaign")+"/checkpoint-checks.txt";
    bool previousExists,restored;int previous;
    public string Status{get;private set;}="running";
    public static void Run(){new GameObject("Temporary checkpoint verification").AddComponent<CampaignCheckpointProbe>().StartCoroutine("Execute");}
    void Check(bool condition,string message){if(!condition){Status="FAIL "+message;File.AppendAllText(Out,Status+"\n");RestorePreference();throw new Exception(Status);}File.AppendAllText(Out,"PASS "+message+"\n");}
    IEnumerator Execute()
    {
        previousExists=PlayerPrefs.HasKey(Key);previous=PlayerPrefs.GetInt(Key);File.WriteAllText(Out,"");
        var session=FindFirstObjectByType<GameSession>();var director=FindFirstObjectByType<CampaignDirector>();var loop=FindFirstObjectByType<LoopController>();var player=FindFirstObjectByType<FirstPersonController>();var recorder=player.GetComponent<EchoRecorder>();director.SuppressSaves=true;
        session.BeginOrResume();player.enabled=false;
        director.BeginExpansionProbe();director.SuppressSaves=false;
        foreach(var station in FindObjectsByType<CampaignStation>(FindObjectsSortMode.None))if(station.Kind==StationKind.PressureOrigin)station.Interact(player.gameObject);
        Check(PlayerPrefs.GetInt(Key)==2,"actual chapter-entry interaction writes the continuation checkpoint");director.SuppressSaves=true;
        PlayerPrefs.SetInt(Key,999);Check(!director.HasSavedCheckpoint&&!director.RestoreCheckpoint(),"out-of-range saved checkpoint rejected");
        for(int i=2;i<=5;i++)
        {
            PlayerPrefs.SetInt(Key,i);Check(director.HasSavedCheckpoint&&director.RestoreCheckpoint(),"restored checkpoint "+i);
            Check((int)director.Chapter==i,"chapter state matches saved checkpoint "+i);
            Check(Vector3.Distance(player.transform.position,loop.RecallSpawnPosition)<.01f,"Player and recall origin agree "+i);
            Check(loop.LastRecording==null&&loop.ActiveEcho==null,"continuation invents no past Echo "+i);
            yield return new WaitForSeconds(.15f);
            Check(recorder.CapturedFrameCount==0,"teleport into continuation is not recorded "+i);
            Check(i==3?!loop.enabled:loop.enabled,"recording availability restored "+i);
            if(i>=4)Check(director.DocumentsRead==7&&director.CircuitsRestored==3,"completed house prerequisites restored "+i);
            if(i==5)Check(director.GalleryPairs==3&&!director.HasFinalWitness,"archive keeps gallery progress but requires a fresh witness");
        }
        RestorePreference();Status="PASS";File.AppendAllText(Out,"PASS saved user preference restored after test\n");session.SetPaused(true);Debug.Log("Campaign checkpoint probe PASS");
    }
    void RestorePreference(){if(restored)return;restored=true;if(previousExists)PlayerPrefs.SetInt(Key,previous);else PlayerPrefs.DeleteKey(Key);PlayerPrefs.Save();}
    void OnDestroy(){RestorePreference();}
}
#endif
