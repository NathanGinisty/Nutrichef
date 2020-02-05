using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Playlist_GameScene : MonoBehaviour
{
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        GameManager.Instance.Audio.Playlist_AddMusic(sceneName, "YellowCafe");
        GameManager.Instance.Audio.Playlist_AddMusic(sceneName, "Bumbly March");
        GameManager.Instance.Audio.Playlist_AddMusic(sceneName, "Fretless");
        GameManager.Instance.Audio.Playlist_AddMusic(sceneName, "Golly Gee");
    }
}
