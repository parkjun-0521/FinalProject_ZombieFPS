using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    public AudioClip[] bgmClips;    // 배경음 클립 
    public float bgmVolume;         // 배경음 볼륨 
    AudioSource bgmPlayer;          // 

    [Header("SFX")]
    public AudioClip[] sfxClips;    // 효과음 클립 
    public float sfxVolume;         // 효과음 볼륨 
    public int channels;            // 채널 수 
    AudioSource[] sfxPlayers;
    int channelIndex;               //  채널 인텍스 

    public enum Sfx { 
        MediKit,                // 힐팩 스왑 소리
        Player_Death,           // 플레이어 죽음 
        Player_death_BGM,       // 죽은 이후 BGM 
        Player_door,            // 문여는 소리 
        Player_explosion,       // 수류탄 폭발 
        Player_granede,         // 수류탄 던지는 소리 
        Player_gun2,            // 총 소리     
        Player_help,            // 기절 소리 
        Player_hurt,            // 피격 소리 
        Player_item,            // 아이템 습득 소리 
        Player_jump,            // 점프 소리 
        Player_knife = 12,      // 11,12 칼 소리 ( 랜덤 ) 
        Player_run2,            // 달리기 
        Player_walk3,           // 걷기 
        UI_Button,              // UI 버튼 
        Zombie_attack,          // 좀비 공격 0
        Zombie_attack1,         // 좀비 공격 1
        Zombie_attack2,         // 좀비 공격 2
        Zombie_attack3,         // 좀비 공격 3
        Zombie_attack4,         // 좀비 공격 4
        Zombie_attack5,         // 좀비 공격 5
        Zombie_attack6,         // 좀비 공격 6
        Zombie_explosion = 24,  // 24, 25 좀비 터지는 소리 랜덤 
        Zombie_hurt = 26,       // 26, 27 좀비 피격 소리 랜덤 
        Zombie_run,             // 좀비 달리기 
        Zombie_walk,            // 좀비 걷기 
        Player_reload3,         // 플레이어 장전
        Player_Door,            // 플레이어 문
        Bandage,                // 플레이어 붕대 감는 소리 
        BossDie1,               // 보스 죽는 소리 1
        BossDie2,               // 보스 죽는 소리 2
        BossHit = 37,           // 보스 피격 소리 0~4
        BossRun1,               // 보스 추격 소리 1
        BossRun2,               // 보스 추격 소리 2
        BossWalk1,              // 보스 걷는 소리 1
        BossWalk2,              // 보스 걷는 소리 2
        GrenadePinRull,         // 플레이어 수류탄 스왑
        Medikit2,               // 플레이어 붕대 감는 소리 
        Zombie_dead1 = 47,      // 좀비 죽는 소리 
    }

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
        bgmPlayer.clip = bgmClips[0];

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

    public void PlayBgm(bool isPlay, int stageCount)
    {
        // 사용시 AudioManager.Instance.PlayBgm(true); // 끌때는 false
        if (isPlay) {
            if (stageCount == 0)
                bgmPlayer.clip = bgmClips[0];
            else if (stageCount == 1) {
                bgmPlayer.clip = bgmClips[1];
                SetBgmVolume(0.8f);
            }
            else if (stageCount == 2)
                bgmPlayer.clip = bgmClips[2];
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
            if (sfx == Sfx.Player_knife || sfx == Sfx.Zombie_explosion || sfx == Sfx.Zombie_hurt) {
                randIndex = Random.Range(0, 2);
            }
            else if (sfx == Sfx.BossHit || sfx == Sfx.Zombie_dead1) {
                randIndex = Random.Range(0, 4);
            }

            channelIndex = loopIndex;

            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx - randIndex];
            sfxPlayers[loopIndex].Play();

            break;
        }
    }

    public bool IsPlaying( Sfx sfx ) {
        foreach (var player in sfxPlayers) {
            if (player.isPlaying && player.clip == sfxClips[(int)sfx]) {
                return true;
            }
        }
        return false;
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = volume;
        bgmPlayer.volume = bgmVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
        foreach (AudioSource player in sfxPlayers) {
            player.volume = sfxVolume;
        }
    }
}
