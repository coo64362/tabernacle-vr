using UnityEngine;

// Transfers each actual playback frame once from Unity's audio thread to the
// speech analyser. Fixed storage avoids allocating or invoking Unity APIs there.
public sealed class SpeechAudioTap : MonoBehaviour {
 const int Capacity=8, Block=2048;
 readonly float[][] blocks=new float[Capacity][];
 readonly int[] lengths=new int[Capacity];readonly object gate=new object();
 int read,write,count;bool accepting;long receivedFrames;float peakSample;
 public long ReceivedFrames {get{lock(gate)return receivedFrames;}}
 public float PeakSample {get{lock(gate)return peakSample;}}
 void Awake(){for(int i=0;i<Capacity;i++)blocks[i]=new float[Block];}
 public void SetAccepting(bool value){lock(gate){accepting=value;if(!value)read=write=count=0;}}
 void OnAudioFilterRead(float[] data,int channels){
  if(channels<1)return;
  lock(gate){
   if(!accepting||blocks[0]==null)return;
   int frames=data.Length/channels;receivedFrames+=frames;
   for(int start=0;start<frames;start+=Block){
    if(count==Capacity){read=(read+1)%Capacity;count--;}
    int size=System.Math.Min(Block,frames-start);var target=blocks[write];
    for(int i=0;i<size;i++){float sum=0;for(int c=0;c<channels;c++)sum+=data[(start+i)*channels+c];target[i]=sum/channels;peakSample=System.Math.Max(peakSample,System.Math.Abs(target[i]));}
    lengths[write]=size;write=(write+1)%Capacity;count++;
   }
  }
 }
 public bool TryRead(float[] destination,out int size){
  lock(gate){size=0;if(count==0)return false;size=lengths[read];System.Array.Copy(blocks[read],destination,size);read=(read+1)%Capacity;count--;return true;}
 }
 void OnDisable()=>SetAccepting(false);
}
