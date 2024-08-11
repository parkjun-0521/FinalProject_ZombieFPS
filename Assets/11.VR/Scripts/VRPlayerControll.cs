using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class VRPlayerControll : MonoBehaviour
{
    Rigidbody rigid;
    public float lastAttackTime = 0.0f; // ������ ���� �ð� 
    public float attackMaxDelay = 0.1f; // ���Ÿ� ���� ������ ( ������ ������ ���� weapon�� ����� �ű⼭ �ҷ��ͼ� ���� ) 
    public bool isJump;              // ���� ����
    public float jumpForce;

    /*public XRController controller;*/
    private XRController xrController;

    private void Awake()
    {
        rigid = GetComponentInChildren<Rigidbody>();
        xrController = GameObject.Find("XR Origin").transform.GetChild(0).GetChild(1).GetComponent<XRController>();
    }
 
    // Start is called before the first frame update
    void Start()
    {
       
    }


    // Update is called once per frame
    void Update()
    {
        if (xrController.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool AButton))
        {
            if(AButton==true)
            {
                rigid.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                AudioManager.Instance.PlayerSfx(AudioManager.Sfx.Player_jump);
                isJump = false;

            }
           
        }
    }

    void OnCollisionExit(Collision collision)
    {
        //if (PV.IsMine)
        {
            if (collision.gameObject.CompareTag("Ground") && rigid.velocity.y > 0)
            {
                isJump = false;
            }
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        //if (PV.IsMine)
        {
            // ���� �±� �ʿ� 
            if (collision.gameObject.CompareTag("Ground"))
            {
                isJump = true;
            }
        }
    }

}
