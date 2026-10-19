#if UNITY_EDITOR || ECHO_NATIVE_VERIFY
using System;using System.IO;using System.Collections.Generic;using System.Linq;using System.Threading;
using UnityEngine;using EchoProtocol.Audio;using EchoProtocol.Presentation;using EchoProtocol.Campaign;
// Passive post-SessionAudio listener capture in QA only. Background disk writes,
// bounded preallocated buffers, no camera or Player movement, no automatic resume.
public sealed class ResonanceRouteObserver : MonoBehaviour
{
    readonly object sync=new object();readonly Queue<float[]> free=new Queue<float[]>();readonly Queue<float[]> pending=new Queue<float[]>();
    readonly AutoResetEvent wake=new AutoResetEvent(false);readonly List<object> states=new List<object>();readonly List<float> frames=new List<float>();
    Thread writer;volatile bool stopping;volatile bool live;int rate,channels,blockLength,dropped,shapeMismatch,bufferStarved;long used;string output,error;float next;bool finished;
    ResonanceSequence sequence;ResonanceEmitter[] emitters;GameSession session;SessionAudio mix;CampaignDirector campaign;string mark="";
    public string FixtureStep { get; set; } = "focused acoustic fixture";
    public double CapturedSeconds { get { lock(sync) return (double)used/channels/rate; } }
    public static ResonanceRouteObserver Begin(string directory)
    {var listener=FindFirstObjectByType<AudioListener>();var o=listener.gameObject.AddComponent<ResonanceRouteObserver>();o.Setup(directory);return o;}
    void Setup(string directory)
    {
        output=directory;Directory.CreateDirectory(output);rate=AudioSettings.outputSampleRate;AudioSettings.GetDSPBufferSize(out int dsp,out _);channels=2;
        if(AudioSettings.GetConfiguration().speakerMode!=AudioSpeakerMode.Stereo)throw new InvalidOperationException("This QA capture requires the actual stereo device output; mono is tested through SessionAudio.");
        blockLength=dsp*channels;for(int i=0;i<512;i++)free.Enqueue(new float[blockLength]);
        sequence=FindFirstObjectByType<ResonanceSequence>();emitters=FindObjectsByType<ResonanceEmitter>(FindObjectsSortMode.None).OrderBy(e=>e.name).ToArray();
        session=FindFirstObjectByType<GameSession>();mix=FindFirstObjectByType<SessionAudio>();campaign=FindFirstObjectByType<CampaignDirector>();
        live=true;writer=new Thread(WriteAudio){IsBackground=true,Name="Echo QA listener capture"};writer.Start();
    }
    void OnAudioFilterRead(float[] data,int c)
    {
        if(!live)return;
        lock(sync){if(c!=channels||data.Length!=blockLength){shapeMismatch++;dropped++;return;}if(free.Count==0){bufferStarved++;dropped++;return;}var b=free.Dequeue();Array.Copy(data,b,data.Length);pending.Enqueue(b);used+=data.Length;}
        wake.Set();
    }
    void WriteAudio()
    {
        try{
            using(var fs=new FileStream(Path.Combine(output,"listener.wav"),FileMode.Create,FileAccess.Write,FileShare.Read,1048576))using(var w=new BinaryWriter(fs)){
                w.Write(new byte[44]);var pcm=new byte[blockLength*2];long samples=0;
                while(true){float[] b=null;lock(sync){if(pending.Count>0)b=pending.Dequeue();else if(stopping)break;}
                    if(b==null){wake.WaitOne(100);continue;}
                    for(int i=0;i<b.Length;i++){float v=b[i];if(float.IsNaN(v)||float.IsInfinity(v)){error="non-finite output";v=0;}short q=(short)Math.Round(Math.Max(-1,Math.Min(1,v))*32767);pcm[2*i]=(byte)q;pcm[2*i+1]=(byte)(q>>8);}
                    w.Write(pcm);samples+=b.Length;lock(sync)free.Enqueue(b);
                }
                fs.Seek(0,SeekOrigin.Begin);w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write((int)(36+samples*2));w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)channels);w.Write(rate);w.Write(rate*channels*2);w.Write((short)(channels*2));w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write((int)(samples*2));
            }
        }catch(Exception ex){error=ex.ToString();}
    }
    void Update()
    {
        if(finished||!live)return;
        var probe=FindFirstObjectByType<ThresholdCampaignProbe>();
        string step=probe?probe.CurrentStep:FixtureStep;
        if(session&&!session.IsPaused)frames.Add(Time.unscaledDeltaTime*1000);
        string state=step+"|"+(sequence?sequence.LastCue:"")+"|"+(session&&session.IsPaused);
        if(Time.realtimeSinceStartup>=next||state!=mark){next=Time.realtimeSinceStartup+.25f;mark=state;long samples;lock(sync)samples=used;
            states.Add(new{seconds=(double)samples/channels/rate,gameTime=Time.time,step,paused=session&&session.IsPaused,started=session&&session.HasStarted,chapter=campaign?campaign.Chapter.ToString():"",cue=sequence?sequence.LastCue:"",cueCount=sequence?sequence.CueCount:0,recalls=sequence?sequence.RecallCount:0,volume=AudioListener.volume,mono=mix&&mix.Speakers,reduced=mix&&mix.ReducedDynamics,position=new[]{transform.position.x,transform.position.y,transform.position.z},forward=new[]{transform.forward.x,transform.forward.y,transform.forward.z},emitters=emitters.Select(e=>new{name=e.name,playing=e.IsPlaying,gain=e.AppliedGain,obstructed=e.Obstructed,cutoff=e.Cutoff,plays=e.PlayCount,sample=e.Source.timeSamples,position=new[]{e.transform.position.x,e.transform.position.y,e.transform.position.z}}).ToArray()});
        }
        if(probe&&(probe.Status=="PASS"||probe.Status.StartsWith("FAIL")))Finish(probe.Status);
    }
    public void Finish(string status)
    {
        if(finished)return;finished=true;live=false;stopping=true;wake.Set();bool joined=writer==null||writer.Join(5000);frames.Sort();
        File.WriteAllText(Path.Combine(output,"audio-route.json"),Newtonsoft.Json.JsonConvert.SerializeObject(new{status,rate,channels,dropped,shapeMismatch,bufferStarved,captureBufferBlocks=512,writerJoined=joined,error,sampleCount=used,seconds=(double)used/channels/rate,scope="Actual post-SessionAudio listener callback during declared route. No source soloing, gain normalization, camera placement or hidden resume. OS/hardware gain and subjective listening not established.",frameCount=frames.Count,meanMs=frames.Count>0?frames.Average():0,p95Ms=frames.Count>0?frames[(int)(frames.Count*.95f)]:0,states},Newtonsoft.Json.Formatting.Indented));
    }
    void OnDestroy(){if(!finished&&output!=null)Finish("interrupted");wake.Dispose();}
}
#endif
