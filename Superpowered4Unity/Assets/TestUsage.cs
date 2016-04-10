using UnityEngine;
using System.Collections;

public class TestUsage : MonoBehaviour {

    int[] Clips = new int[2];
    int clip_counter = 0;
	// Use this for initialization
	void Start () {
        SuperpoweredManager.Instance.Init();
        //here clips 0 and 1 match the handle 0 and 1 but if we register the sounds from more than one script , we may have different handles and not sequential
        Clips[0]=SuperpoweredManager.Instance.RegisterSoundFile("Sound2.wav");
        Clips[1]=SuperpoweredManager.Instance.RegisterSoundFile("Piano 1.wav");
    }

   //The example just play 2 sounds alternating for each touch of the screen
    void Update () {
        //on mouse click we play The clip registered at 
        if (Input.GetMouseButtonDown(0)) {
            print("playing clip n." + clip_counter);
            SuperpoweredManager.Instance.PlaySoundAt(Clips[clip_counter]);

             clip_counter++;
            if (clip_counter >= Clips.Length) clip_counter = 0;
            //Clips[0]
        }
	}
}
