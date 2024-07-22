using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    public AudioClip bgmClip;       // ����� Ŭ�� 
    public float bgmVolume;         // ����� ���� 
    AudioSource bgmPlayer;          // 

    [Header("SFX")]
    public AudioClip[] sfxClips;     // ȿ���� Ŭ�� 
    public float sfxVolume;         // ȿ���� ���� 
    public int channels;            // ä�� �� 
    AudioSource[] sfxPlayers;
    int channelIndex;               //  ä�� ���ؽ� 

    // Sfx Ŭ���� ���� �ְ� �� ���� ������� enum �ۼ� 
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
        // ����� 
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;

        // ȿ���� 
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
        // ���� AudioManager.Instance.PlayerBgm(true); // ������ false
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
