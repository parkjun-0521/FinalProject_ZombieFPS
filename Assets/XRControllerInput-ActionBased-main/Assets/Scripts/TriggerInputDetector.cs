using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using TMPro;

public class TriggerInputDetector : MonoBehaviour
{
     

    private InputData _inputData;
    private float _leftMaxScore = 0f;
    private float _rightMaxScore = 0f;
    private float triggerValue;

    Rigidbody rigid;
    public float lastAttackTime = 0.0f; // ������ ���� �ð� 
    public float attackMaxDelay = 0.1f; // ���Ÿ� ���� ������ ( ������ ������ ���� weapon�� ����� �ű⼭ �ҷ��ͼ� ���� ) 
    public bool isJump;              // ���� ����
    public float jumpForce;

    private void Awake()
    {
        rigid = GameObject.Find("XR Origin (XR Rig)").transform.GetChild(1).GetComponent<Rigidbody>();

    }
    private void Start()
    {
        _inputData = GetComponent<InputData>();
    }
    // Update is called once per frame
    void Update()
    {
        //if (_inputData._leftController.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        //{
        //    //_leftMaxScore = theFloat;
        //    leftScoreDisplay.text = triggerValue.ToString("#.00");
        //    Debug.Log("triggerValue: " + triggerValue);
        //}

        if (_inputData._leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool Abutton))
        {
            //_leftMaxScore = theFloat;
            rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_jump);
            isJump = false;
        }

        if (_inputData._rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool Bbutton))
        {
            Debug.Log("B button: " + Bbutton);
        }

        //triggerValue = ((float)_inputData._leftController.characteristics);
        //Debug.Log("triggerValue is: " + triggerValue);

        //if (_inputData._leftController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 leftVelocity))
        //{
        //    _leftMaxScore = Mathf.Max(leftVelocity.magnitude, _leftMaxScore);
        //    leftScoreDisplay.text = _leftMaxScore.ToString("F2");
        //}
        //if (_inputData._rightController.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 rightVelocity))
        //{
        //    _rightMaxScore = Mathf.Max(rightVelocity.magnitude, _rightMaxScore);
        //    rightScoreDisplay.text = _rightMaxScore.ToString("F2");
        //}
    }
}
