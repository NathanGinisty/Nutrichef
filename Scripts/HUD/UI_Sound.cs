using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Sound : MonoBehaviour
{
    public void PlaySoundHUD(string _name)
    {
        GameManager.Instance.Audio.PlaySound(_name, AudioManager.Canal.SoundEffect);
    }
}
