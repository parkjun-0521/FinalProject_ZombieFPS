using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    public AudioClip bgmClip;       // 배경음 클립 
    public float bgmVolume;         // 배경음 볼륨 
    AudioSource bgmPlayer;          // 

    [Header("SFX")]
    public AudioClip[] sfxClips;     // 효과음 클립 
    public float sfxVolume;         // 효과음 볼륨 
    public int channels;            // 채널 수 
    AudioSource[] sfxPlayers;
    int channelIndex;               //  채널 인텍스 

    // Sfx 클립에 사운드 넣고 그 사운드 순서대로 enum 작성 
    public enum Sfx { Dead, Hit, Walk, Run, Jump, Shot, Melee, Granede, Heal }

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }

        Init();
    }

    void Init()
    {
        // 배경음 
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;

        // 효과음 
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for(int i = 0; i < sfxPlayers.Length; i++) {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].volume = sfxVolume;
        }
    }

    public void PlayBgm(bool isPlay)
    {
        // 사용시 AudioManager.Instance.PlayerBgm(true); // 끌때는 false
        if (isPlay) {
            bgmPlayer.Play();
        }
        else {
            bgmPlayer.Stop();
        }
    }

    public void PlayerSfx(Sfx sfx)
    {
        for(int i = 0; i < sfxPlayers.Length; i++) {
            int loopIndex = (i + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            int randIndex = 0;
            if(sfx == Sfx.Hit || sfx == Sfx.Melee) {
                randIndex = Random.Range(0, 2);
            }

            channelIndex = loopIndex;

            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + randIndex];
            sfxPlayers[loopIndex].Play();

            break;
        }
    }
}
