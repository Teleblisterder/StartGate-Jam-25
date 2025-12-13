using UnityEngine;

public class UsePortal : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bluePortalPrefab;
    [SerializeField] private GameObject orangePortalPrefab;
    [SerializeField] private LayerMask wallLayer;

    private GameObject currentBluePortal;
    private GameObject currentOrangePortal;
    private bool fireBlueNext = true;
    private bool inputLock = false;

    [Header("Ayarlar")]
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private float rotationOffset = 90f; // Inspector'dan ayarlanabilsin diye SerializeField yaptým

    void Update()
    {
        if (InputManager.portalPressed)
        {
            if (!inputLock)
            {
                Shoot();
                inputLock = true;
            }
        }
        else
        {
            inputLock = false;
        }
    }

    void Shoot()
    {
        // 1. Ham yönü al (Bu þu an çapraz olabiliyor, örn: 0.7, 0.7)
        Vector2 rawDirection = playerMovement.FacingDirection;

        // 2. Yönü ana yönlere (4 yöne) yuvarla
        Vector2 finalDirection = GetCardinalDirection(rawDirection);

        Vector2 origin = firePoint != null ? firePoint.position : transform.position;

        // 3. Raycast'i artýk 'finalDirection' ile atýyoruz
        RaycastHit2D hit = Physics2D.Raycast(origin, finalDirection, rayDistance, wallLayer);

        // Debug çizgisini de güncelledim, artýk tam 4 yöne çizilecek
        Debug.DrawRay(origin, finalDirection * rayDistance, Color.cyan, 0.2f);

        if (hit.collider != null)
        {
            if (fireBlueNext)
            {
                SpawnBluePortal(hit.point, hit.normal);
            }
            else
            {
                SpawnOrangePortal(hit.point, hit.normal);
            }

            fireBlueNext = !fireBlueNext;
        }
    }

    void SpawnBluePortal(Vector2 position, Vector2 normal)
    {
        if (currentBluePortal != null) Destroy(currentBluePortal);

        currentBluePortal = InstantiatePortal(bluePortalPrefab, position, normal);

        // --- EKLENEN KISIM: BAÐLANTIYI KUR ---
        UpdatePortalLinks();
    }

    void SpawnOrangePortal(Vector2 position, Vector2 normal)
    {
        if (currentOrangePortal != null) Destroy(currentOrangePortal);

        currentOrangePortal = InstantiatePortal(orangePortalPrefab, position, normal);

        // --- EKLENEN KISIM: BAÐLANTIYI KUR ---
        UpdatePortalLinks();
    }

    GameObject InstantiatePortal(GameObject prefab, Vector2 pos, Vector2 normal)
    {
        Vector2 spawnPos = pos + (normal * 0.1f);
        Quaternion baseRot = Quaternion.FromToRotation(Vector2.up, normal);
        Quaternion finalRot = baseRot * Quaternion.Euler(0, 0, rotationOffset);

        return Instantiate(prefab, spawnPos, finalRot);
    }

    // --- YENÝ EKLENEN FONKSÝYON ---
    // Bu fonksiyon sahnedeki mavi ve turuncu portalý bulup birbirine tanýtýr
    void UpdatePortalLinks()
    {
        // Ýkisi de sahnede var mý?
        if (currentBluePortal != null && currentOrangePortal != null)
        {
            // Prefablarýn üzerindeki "Portal" scriptlerini al
            Portal blueScript = currentBluePortal.GetComponent<Portal>();
            Portal orangeScript = currentOrangePortal.GetComponent<Portal>();

            // Scriptler bulunduysa birbirine eþle
            if (blueScript != null && orangeScript != null)
            {
                blueScript.linkedPortal = orangeScript;
                orangeScript.linkedPortal = blueScript;

                Debug.Log("Baðlantý Baþarýlý: Mavi <-> Turuncu");
            }
        }
    }
    Vector2 GetCardinalDirection(Vector2 direction)
    {
        // Eþik deðeri (Deadzone): Joystick veya klavye hassasiyeti için
        float threshold = 0.1f;

        // KURAL: Eðer X ekseninde (Sað/Sol) belirgin bir hareket varsa,
        // Y ekseni ne olursa olsun Yatay ateþ etmeyi seç.
        if (Mathf.Abs(direction.x) > threshold)
        {
            // Sadece Saða (1,0) veya Sola (-1,0) döndür
            return new Vector2(Mathf.Sign(direction.x), 0);
        }
        // Eðer X ekseninde hareket yoksa (veya çok azsa), o zaman Dikey'e bak
        else
        {
            // Sadece Yukarý (0,1) veya Aþaðý (0,-1) döndür
            // (Mathf.Sign 0 için 1 döndürür, o yüzden Y de 0 ise hata olmasýn diye kontrol ekledik)
            if (Mathf.Abs(direction.y) > threshold)
                return new Vector2(0, Mathf.Sign(direction.y));
            else
                return playerMovement.FacingDirection; // Hiç hareket yoksa son yöne devam
        }
    }
}