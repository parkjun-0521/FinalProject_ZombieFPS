using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputKeyManager : MonoBehaviour {
    public static InputKeyManager instance;

    // 열거형 변수 선언 
    public enum KeyCodeTypes {
        LeftMove,
        RightMove,
        DownMove,
        UpMove,
        Down,
        Run,
        Jump,
        Attack,
        Interaction,
        Weapon1,
        Weapon2,
        Weapon3,
        Weapon4
    }

    // 딕셔너리로 키 관리 
    private Dictionary<KeyCodeTypes, KeyCode> keyMappings;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
            return;
        }
        // 딕셔너리 초기화 
        keyMappings = new Dictionary<KeyCodeTypes, KeyCode>();

        // 각 디셔너리 키에 맞는 키보드 값을 추가 
        keyMappings[KeyCodeTypes.LeftMove] = KeyCode.A;
        keyMappings[KeyCodeTypes.RightMove] = KeyCode.D;
        keyMappings[KeyCodeTypes.DownMove] = KeyCode.S;
        keyMappings[KeyCodeTypes.UpMove] = KeyCode.W;
        keyMappings[KeyCodeTypes.Run] = KeyCode.LeftShift;
        keyMappings[KeyCodeTypes.Jump] = KeyCode.Space;
        keyMappings[KeyCodeTypes.Attack] = KeyCode.Mouse0;
        keyMappings[KeyCodeTypes.Interaction] = KeyCode.E;
        keyMappings[KeyCodeTypes.Weapon1] = KeyCode.Alpha1;     // 원거리 무기 
        keyMappings[KeyCodeTypes.Weapon2] = KeyCode.Alpha2;     // 근거리 무기
        keyMappings[KeyCodeTypes.Weapon3] = KeyCode.Alpha3;     // 투척 무기 
        keyMappings[KeyCodeTypes.Weapon4] = KeyCode.Alpha4;     // 힐팩 
    }

    public KeyCode GetKeyCode( KeyCodeTypes action ) {
        // 키값 반환 
        return keyMappings[action];
    }

    public void SetKeyCode( KeyCodeTypes action, KeyCode keyCode ) {
        // 키값 설정 
        keyMappings[action] = keyCode;
    }
}
