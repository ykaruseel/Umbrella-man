using UnityEngine;
using UnityEngine.Video;

public class VideoLoader : MonoBehaviour
{
    public VideoPlayer player;

    void Start()
    {
        player.url = System.IO.Path.Combine(Application.streamingAssetsPath, "vhsnoise.mp4");
        player.Play();
    }
}