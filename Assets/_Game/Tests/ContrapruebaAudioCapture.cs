#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.IO;using UnityEngine;
// Appended after SessionAudio on the real listener. Captures the signal emitted
// during actual input routes; device loudness and comfort require human review.
public sealed class ContrapruebaAudioCapture:MonoBehaviour
{
 readonly object sync=new object();float[] samples;int used,channels,rate;bool live;
 readonly List<object> marks=new List<object>();
 public void Begin(){rate=AudioSettings.outputSampleRate;samples=new float[rate*2*240];used=0;live=true;}
 public void Mark(string label){lock(sync)marks.Add(new{label,sample=used,seconds=channels>0?(double)used/channels/rate:0});}
 void OnAudioFilterRead(float[] data,int count){lock(sync){if(!live)return;channels=count;int n=Math.Min(data.Length,samples.Length-used);Array.Copy(data,0,samples,used,n);used+=n;}}
 public void Save(string path){lock(sync){live=false;if(channels<1||used==0)throw new Exception("No real listener audio was captured");Directory.CreateDirectory(Path.GetDirectoryName(path));using(var w=new BinaryWriter(File.Create(path))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+used*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)channels);w.Write(rate);w.Write(rate*channels*2);w.Write((short)(channels*2));w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(used*2);for(int i=0;i<used;i++)w.Write((short)Mathf.RoundToInt(Mathf.Clamp(samples[i],-1,1)*32767));}File.WriteAllText(path+".json",Newtonsoft.Json.JsonConvert.SerializeObject(new{rate,channels,samples=used,marks,scope="Actual listener output; source position, reduction and mono require accompanying state checks. Not human listening or hardware volume evidence."},Newtonsoft.Json.Formatting.Indented));samples=null;}}
}
#endif
