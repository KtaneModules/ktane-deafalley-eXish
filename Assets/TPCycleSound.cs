using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPCycleSound : MonoBehaviour {

    public KMAudio Audio;
    KMAudio.KMAudioRef sound;

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("In");
        Debug.Log(collision.collider.name);
        if (collision.collider.name != "Highlight")
            sound = Audio.PlaySoundAtTransformWithRef("tone", transform);
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log("Out");
        if (collision.collider.name != "Highlight")
            sound.StopSound();
    }
}
