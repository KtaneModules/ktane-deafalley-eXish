using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPCycleSound : MonoBehaviour {

    public KMAudio Audio;
    KMAudio.KMAudioRef sound;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name != "Highlight")
            sound = Audio.PlaySoundAtTransformWithRef("tone", transform);
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.name != "Highlight")
            sound.StopSound();
    }
}
