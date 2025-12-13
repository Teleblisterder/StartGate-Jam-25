using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform spawnPoint;
    public float cooldownTime = 0.5f;

    [HideInInspector] public Portal linkedPortal; // PortalGun burayý dolduruyor mu?

    private bool isCoolingDown = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // --- SORUN GÝDERME AJANLARI (DEBUG) ---
        // Konsola kimin çarptýðýný yazdýrýr
        Debug.Log("Portala giren obje: " + other.name + " | Tag: " + other.tag);

        if (isCoolingDown)
        {
            Debug.Log("Portal soðuma süresinde.");
            return;
        }

        if (linkedPortal == null)
        {
            Debug.Log("HATA: Diðer portal baðlý deðil! (LinkedPortal NULL)");
            return;
        }
        // --------------------------------------

        // ARTIK SADECE PLAYER DEÐÝL, KUTULARI DA KABUL EDÝYORUZ
        // Ýstersen buraya || other.CompareTag("Enemy") vs. ekleyebilirsin.
        if (other.CompareTag("Player") || other.CompareTag("Box"))
        {
            Debug.Log("Iþýnlanma Baþlýyor!");
            Teleport(other);
        }
        else
        {
            Debug.Log("Bu obje ýþýnlanma listesinde yok.");
        }
    }

    private void Teleport(Collider2D traveler)
    {
        Rigidbody2D rb = traveler.GetComponent<Rigidbody2D>();

        if (linkedPortal != null)
        {
            linkedPortal.StartCoroutine(linkedPortal.CooldownRoutine());

            // --- DEÐÝÞÝKLÝK BURADA ---
            // Y ekseni (Boy) yerine X eksenini (Geniþlik) alýyoruz.
            // Böylece kutunun geniþliðine göre önüne mesafe koyar.
            float objectHalfSize = traveler.bounds.extents.x;

            /* ÝPUCU: Eðer kutun dönebilen bir kareyse ve bazen sýðmýyorsa,
               garanti olsun diye þu satýrý kullanabilirsin (En büyük kenarý alýr):
               float objectHalfSize = Mathf.Max(traveler.bounds.extents.x, traveler.bounds.extents.y); 
            */

            float safetyBuffer = 0.2f; // Ekstra güvenli mesafe

            // ÖNEMLÝ NOT: Burada hala 'spawnPoint.up' kullanýyoruz.
            // Neden? Çünkü Portal döndürüldüðü için, portalýn "ÖNÜ" (Çýkýþ yönü) 
            // her zaman onun yerel 'Yukarý' (Yeþil Ok) yönüdür.
            Vector3 exitDirection = linkedPortal.spawnPoint.right;

            // Objeyi çýkýþ yönünde, kendi geniþliði kadar ileri taþý
            Vector3 targetPosition = linkedPortal.spawnPoint.position + (exitDirection * (objectHalfSize + safetyBuffer));

            traveler.transform.position = targetPosition;

            // Hýz vektörünü ayarla
            if (rb != null)
            {
                float currentSpeed = rb.linearVelocity.magnitude; // Unity 6 (eskiyse rb.velocity)
                float exitSpeed = Mathf.Max(currentSpeed, 5f); // Minimum fýrlatma hýzý

                rb.linearVelocity = exitDirection * exitSpeed;
            }
        }
    }

    public IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCoolingDown = false;
    }
}