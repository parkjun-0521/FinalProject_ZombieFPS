using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputKeyManager : MonoBehaviour {
    public static InputKeyManager instance;

    // ������ ���� ���� 
    public enum KeyCodeTypes {
        LeftMove,
        RightMove,
        DownMove,
        UpMove,
        Down,
        Run,
        Jump,
        Attack
    }

    // ��ųʸ��� Ű ���� 
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
        // ��ųʸ� �ʱ�ȭ 
        keyMappings = new Dictionary<KeyCodeTypes, KeyCode>();

        // �� ��ųʸ� Ű�� �´� Ű���� ���� �߰� 
        keyMappings[KeyCodeTypes.LeftMove] = KeyCode.LeftArrow;
        keyMappings[KeyCodeTypes.RightMove] = KeyCode.RightArrow;
        keyMappings[KeyCodeTypes.DownMove] = KeyCode.DownArrow;
        keyMappings[KeyCodeTypes.UpMove] = KeyCode.UpArrow;
        keyMappings[KeyCodeTypes.Run] = KeyCode.LeftShift;
        keyMappings[KeyCodeTypes.Jump] = KeyCode.Space;
        keyMappings[KeyCodeTypes.Attack] = KeyCode.Mouse0;
    }

    public KeyCode GetKeyCode( KeyCodeTypes action ) {
        // Ű�� ��ȯ 
        return keyMappings[action];
    }

    public void SetKeyCode( KeyCodeTypes action, KeyCode keyCode ) {
        // Ű�� ���� 
        keyMappings[action] = keyCode;
    }
}
