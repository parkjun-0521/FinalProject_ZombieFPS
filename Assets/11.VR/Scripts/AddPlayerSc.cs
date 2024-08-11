using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AddPlayerSc : MonoBehaviour
{
    public GameObject bulletPrefab; // ÃÑ¾Ë ÇÁ¸®ÆÕ
    public Transform bulletSpawnPoint; // ÃÑ¾Ë ¹ß»ç À§Ä¡
    public float bulletSpeed = 20f; // ÃÑ¾Ë ¼Óµµ

    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.activated.AddListener(Shoot);
    }

    void OnDestroy()
    {
        grabInteractable.activated.RemoveListener(Shoot);
    }

    void Shoot(ActivateEventArgs args)
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = bulletSpawnPoint.forward * bulletSpeed;

        // Debugging line to visualize the direction
        Debug.DrawRay(bulletSpawnPoint.position, bulletSpawnPoint.forward * 2, Color.red, 2f);
    }
}
