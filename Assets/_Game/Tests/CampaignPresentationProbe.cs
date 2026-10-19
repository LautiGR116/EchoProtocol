#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using UnityEngine;
using EchoProtocol.Campaign;
using EchoProtocol.Player;
using EchoProtocol.Presentation;

// Explicitly staged visual QA. This is not the full-route verification.
public sealed class CampaignPresentationProbe:MonoBehaviour
{
    string Out=>"Docs/VisualEvidence/2026-09-30-"+(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Finish")?"Finish":"Campaign")+"/";
    GameSession session;CampaignDirector director;FirstPersonController player;
    public static void Run(){new GameObject("Temporary staged visual inspection").AddComponent<CampaignPresentationProbe>().StartCoroutine("Execute");}
    void LateUpdate(){if(session&&session.IsPaused&&!session.IsEnding)session.BeginOrResume();}
    CampaignStation Station(StationKind kind)=>FindObjectsByType<CampaignStation>(FindObjectsSortMode.None).First(s=>s.Kind==kind);
    void Use(StationKind kind)
    {
        var station=Station(kind);var p=station.transform.position-station.transform.forward*1.5f;p.y=.03f;
        player.ResetPose(p,station.transform.rotation);station.Interact(player.gameObject);
    }
    IEnumerator Execute()
    {
        yield return null;session=FindFirstObjectByType<GameSession>();director=FindFirstObjectByType<CampaignDirector>();player=FindFirstObjectByType<FirstPersonController>();director.SuppressSaves=true;
        session.BeginOrResume();player.enabled=false;director.PrepareHouseProbe();Use(StationKind.HouseOrigin);Use(StationKind.Telephone);
        Use(StationKind.CircuitKitchen);yield return new WaitForSeconds(4.3f);Use(StationKind.CircuitBedroom);yield return new WaitForSeconds(4.3f);
        player.ResetPose(new Vector3(0,.03f,-54.4f),Quaternion.identity);yield return new WaitForSeconds(.35f);
        ScreenCapture.CaptureScreenshot(Out+"28-presence-final-staged.png");
        yield return new WaitForSeconds(2);Debug.Log("Staged final presentation captured; presence resolved="+director.PresenceResolved+", HUD hidden during sight qualification.");
        player.ResetPose(new Vector3(-3.4f,.03f,-56.3f),Quaternion.Euler(0,222,0));yield return new WaitForSeconds(.2f);
        ScreenCapture.CaptureScreenshot(Out+"29-parlor-final-staged.png");
    }
}
#endif
