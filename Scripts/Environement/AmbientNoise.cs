using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientNoise : MonoBehaviour
{
    [SerializeField] List<string> listSound;

    void Start()
    {
        PlayRandomSound();
    }

    void Update()
    {
        if (GetComponent<AudioSource>() == null)
        {
            PlayRandomSound();
        }
    }

    private void PlayRandomSound()
    {
        int randChoice = Random.Range(0, listSound.Count);
        GameManager.Instance.Audio.PlaySound(listSound[randChoice], AudioManager.Canal.Ambient, gameObject);
    }


}
