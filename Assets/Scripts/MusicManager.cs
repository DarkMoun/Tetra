using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public List<AudioClip> musicList;
    public AudioSource source;

    private List<int> playingOrder;
    private System.Random random;
    private int currentPlaying = 0;

    private List<int> tmpIntList;

    // Start is called before the first frame update
    void Start()
    {
        if (musicList == null)
            musicList = new List<AudioClip>();

        if (musicList.Count == 0)
            enabled = false;

        random = new System.Random();

        playingOrder = new List<int>();
        tmpIntList = new List<int>();

        SetPlayingOrder();
        currentPlaying = 0;
        source.loop = false;
        source.clip = musicList[playingOrder[currentPlaying]];
        source.Play();
    }

    private void SetPlayingOrder()
    {
        if (musicList.Count == 0)
            return;

        for (int i = 0; i < musicList.Count; i++)
            tmpIntList.Add(i);

        int randomValue = -1;
        while(tmpIntList.Count > 0)
        {
            randomValue = random.Next(tmpIntList.Count);
            playingOrder.Add(tmpIntList[randomValue]);
            tmpIntList.RemoveAt(randomValue);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!source.isPlaying)
        {
            currentPlaying = currentPlaying + 1 >= playingOrder.Count ? 0 : currentPlaying + 1;
            source.clip = musicList[playingOrder[currentPlaying]];
            source.Play();
        }
    }
}
