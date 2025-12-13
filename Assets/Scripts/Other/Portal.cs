using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform spawnPoint;
    public float cooldownTime = 0.5f;

    [HideInInspector] public Portal linkedPortal;

    private bool isCoolingDown = false;

    [Header("Hassas Ayarlar")]
    [Tooltip("-1: Tam ZIT (Üstten giren alttan çýkar). 1: AYNI (Üstten giren üstten çýkar).")]
    public float offsetCarpani = -1f; // VARSAYILAN OLARAK ZIT (-1) AYARLADIM

    [Header("Sýkýþma Önleme")]
    [Tooltip("Kutu çýkýnca duvardan ne kadar uzakta oluþsun?")]
    public float ekstraMesafe = 0.8f; // Kutular için güvenli mesafe
    public float minFirlatmaHizi = 14f; // Hýzlý fýrlat ki arkasý sýkýþmasýn

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCoolingDown || linkedPortal == null) return;

        if (other.CompareTag("Player") || other.CompareTag("Box"))
        {
            Teleport(other);
        }
    }

    private void Teleport(Collider2D traveler)
    {
        Rigidbody2D rb = traveler.GetComponent<Rigidbody2D>();

        if (linkedPortal != null)
        {
            linkedPortal.StartCoroutine(linkedPortal.CooldownRoutine());

            // --- 1. GÝRÝÞ NOKTASI HESABI ---
            // Portalýn merkezinden ne kadar yukarýda/aþaðýdayýz? (Dot Product en hatasýz yöntemdir)
            Vector3 distanceVector = traveler.transform.position - transform.position;
            float rawOffset = Vector3.Dot(distanceVector, transform.up);

            // --- 2. ZITLIK / YÖN AYARI (Senin Ýstediðin Yer) ---
            // Buradaki çarpan -1 olduðu sürece TERS taraftan çýkar.
            float finalOffset = rawOffset * offsetCarpani;

            // --- 3. ÇIKIÞ NOKTASININ MERKEZÝ ---
            // Portalýn göbeðinden, hesaplanan offset kadar yukarý/aþaðý git
            Vector3 targetHeightPoint = linkedPortal.transform.position + (linkedPortal.transform.up * finalOffset);

            // --- 4. ÝLERÝ ÝTME (Kutu Sýkýþmasýn) ---
            // Kutunun en kalýn kenarýný bul
            float objectThickness = Mathf.Max(traveler.bounds.extents.x, traveler.bounds.extents.y);

            // Fýrlatma yönü (SpawnPoint Right)
            Vector3 pushDirection = linkedPortal.spawnPoint.right;

            // HESAP: Yükseklik Noktasý + (Yön * (Kalýnlýk + Ekstra Pay))
            Vector3 finalPosition = targetHeightPoint + (pushDirection * (objectThickness + ekstraMesafe));

            // Konumu Uygula
            traveler.transform.position = finalPosition;

            // --- 5. HIZI GÜNCELLE ---
            if (rb != null)
            {
                // Mevcut hýzý al ama en az 'minFirlatmaHizi' olsun
                float exitSpeed = Mathf.Max(rb.linearVelocity.magnitude, minFirlatmaHizi);
                rb.linearVelocity = pushDirection * exitSpeed;
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