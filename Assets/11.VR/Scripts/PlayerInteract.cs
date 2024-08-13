using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerInteract : MonoBehaviour
{
    [Header("==VR 상호작용 배정==")]
    //점프
    [SerializeField] private InputActionProperty jumpButton;
    [SerializeField] private LayerMask groundLayers;
    public float jumpPower;
    [Space(10)]
    //공격
    [SerializeField] private InputActionProperty attackButton;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 20f;
    [Space(10)]
    //문열기
    [SerializeField] private InputActionProperty doorButton;


    [Space(20)]
    //[SerializeField] private CharacterController cc;
    private float gravity = -9.81f;
    private Vector3 velocity;
    bool isGrounded = true;
    Rigidbody rigid;
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (jumpButton.action.WasPerformedThisFrame() && isGrounded)
        {
            isGrounded = false;
            Jump();
            Debug.Log("jumping");
            Debug.Log(velocity.y);
        }
        if (attackButton.action.WasPerformedThisFrame()) //&& onWeapon)
        {
            Shoot();
        }
        if (doorButton.action.WasPerformedThisFrame())
        {

            //Door door = hit.transform.GetComponentInChildren<Door>();
            //if (door.isOpen)
            //{
            //    door.CloseDoorRPC();
            //}
            //else if (!door.isOpen)
            //{
            //    door.OpenDoorRPC();
            //}
        }





    }
    void Jump()
    {
        //transform.Translate(Vector3.up * jumpPower * Time.deltaTime);
        rigid.AddForce(Vector3.up * jumpPower * Time.deltaTime, ForceMode.Impulse);
    }
    
    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = bulletSpawnPoint.forward * bulletSpeed;
        }
        Destroy(bullet, 2f);
    }

}
