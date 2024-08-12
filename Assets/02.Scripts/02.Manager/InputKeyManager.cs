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
        Run,
        Jump,
        Attack,
        Interaction,
        Inventory,
        Weapon1,
        Weapon2,
        Weapon3,
        Weapon4,
        Setting,
        BulletLoad,
        MiniMap
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
        keyMappings[KeyCodeTypes.LeftMove] = KeyCode.A;
        keyMappings[KeyCodeTypes.RightMove] = KeyCode.D;
        keyMappings[KeyCodeTypes.DownMove] = KeyCode.S;
        keyMappings[KeyCodeTypes.UpMove] = KeyCode.W;
        keyMappings[KeyCodeTypes.Run] = KeyCode.LeftShift;
        keyMappings[KeyCodeTypes.Jump] = KeyCode.Space;
        keyMappings[KeyCodeTypes.Attack] = KeyCode.Mouse0;
        keyMappings[KeyCodeTypes.Interaction] = KeyCode.E;
        keyMappings[KeyCodeTypes.Inventory] = KeyCode.Tab;
        keyMappings[KeyCodeTypes.Weapon1] = KeyCode.Alpha1;     // ���Ÿ� ���� 
        keyMappings[KeyCodeTypes.Weapon2] = KeyCode.Alpha2;     // �ٰŸ� ����
        keyMappings[KeyCodeTypes.Weapon3] = KeyCode.Alpha3;     // ��ô ���� 
        keyMappings[KeyCodeTypes.Weapon4] = KeyCode.Alpha4;     // ���� 
        keyMappings[KeyCodeTypes.Setting] = KeyCode.Escape;     // ����
        keyMappings[KeyCodeTypes.BulletLoad] = KeyCode.R;       // ���� 
        keyMappings[KeyCodeTypes.MiniMap] = KeyCode.M;          // ���� 
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
