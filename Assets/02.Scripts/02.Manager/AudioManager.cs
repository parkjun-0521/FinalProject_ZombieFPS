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
    public enum Sfx { 
        MediKit,                // 힐팩 사용
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
        Zombie_dead1,           // 좀비 죽는 소리 
        Zombie_explosion = 25,  // 24, 25 좀비 터지는 소리 랜덤 
        Zombie_hurt = 27,       // 26, 27 좀비 피격 소리 랜덤 
        Zombie_run,             // 좀비 달리기 
        Zombie_walk,            // 좀비 걷기 
        Player_reload3          // 플레이어 장전
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
            if (sfx == Sfx.Player_knife || sfx == Sfx.Zombie_explosion || sfx == Sfx.Zombie_hurt) {
                randIndex = Random.Range(0, 2);
            }

            channelIndex = loopIndex;

            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + randIndex];
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
}
