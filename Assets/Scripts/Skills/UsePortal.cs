using System.Net;
using UnityEngine;
using System.Collections;

public class UsePortal : SkillBase
{
    [Header("Referanslar")]
    // PlayerMovement'ý otomatik bulmasý için OnEnable ekleyeceðiz, ama manuel de atayabilirsin.
    [SerializeField]  private PlayerMovement playerMovement;
    [SerializeField] private GameObject bluePortalPrefab;
    [SerializeField] private GameObject orangePortalPrefab;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField]  private Transform firePoint;

    [Header("Görsel Efektler")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float laserDuration = 0.1f;

    private GameObject currentBluePortal;
    private GameObject currentOrangePortal;
    private Coroutine laserCoroutine;

    [Header("Ayarlar")]
    [SerializeField] private float rayDistance = 10f;
    [SerializeField] private float rotationOffset = 90f;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();

        if (lineRenderer == null)
        {
            // DÝKKAT: Burada GetComponent<LineRenderer>() ASLA KULLANMIYORUZ.
            // Player'ýn içindeki "LaserEffect" isimli çocuðu arýyoruz.
            Transform laserObj = player.transform.Find("LaserEffect");

            if (laserObj != null)
            {
                Debug.Log("2. LaserEffect Objesi Bulundu!");
                lineRenderer = laserObj.GetComponent<LineRenderer>();

                if (lineRenderer != null)
                {
                    lineRenderer.enabled = false;
                    Debug.Log("3. LineRenderer Baþarýyla Alýndý ve Kapatýldý.");
                }
                else
                {
                    Debug.LogError("HATA: 'LaserEffect' objesi var ama üzerinde 'Line Renderer' bileþeni YOK!");
                }
            }
            else
            {
                // Eðer bulamazsa hiyerarþiyi kontrol et
                Debug.LogError("HATA: Player'ýn içinde tam olarak 'LaserEffect' adýnda bir obje bulunamadý! Ýsim hatasý olabilir.");
            }
        }

        // 2. FirePoint Transformunu Bulma:
        // FirePoint'in Player objesinin bir alt objesi olduðunu varsayýyoruz.
        if (firePoint == null)
        {
            Transform playerParent = transform.parent;
            firePoint = playerParent.Find("FirePoint");

        }
    }

    // --- E TUÞU (MAVÝ) ---
    public override void UsePrimary()
    {
        if (!IsReady())
        {
            return;
        }

        // Shoot fonksiyonuna "true" yolluyoruz (Yani: Mavi at)
        Shoot(true);

        nextFireTime = Time.time + cooldownTime;
    }

    // --- Q TUÞU (TURUNCU) ---
    public override void UseSecondary()
    {
        if (!IsReady())
        {
            // Eðer IsReady false döndürürse, yukarýdaki SkillBase metodundan uyarý logu zaten çýkar.
            return; // Hýzlýca çýkýþ yap.
        }
        // Shoot fonksiyonuna "false" yolluyoruz (Yani: Turuncu at)
        Shoot(false);

        nextFireTime = Time.time + cooldownTime;
    }

    // DÝKKAT: Fonksiyon artýk "bool isBlue" parametresi alýyor
    void Shoot(bool isBlue)
    {
        if (playerMovement == null) return;

        // 1. Yön bulma iþlemleri (Aynen korundu)
        Vector2 rawDirection = playerMovement.FacingDirection;
        Vector2 finalDirection = GetCardinalDirection(rawDirection);
        Vector2 origin = firePoint != null ? firePoint.position : transform.position;

        // 2. Raycast at
        RaycastHit2D hit = Physics2D.Raycast(origin, finalDirection, rayDistance, wallLayer);

        Color laserColor = isBlue ? Color.blue : new Color(1f, 0.5f, 0f); // Mavi veya Turuncu
        Vector2 endPoint = hit.collider != null ? hit.point : (origin + finalDirection * rayDistance);
        DrawLaser(origin, endPoint, laserColor);

        // Debug Rengi: Mavi ise Mavi, deðilse Turuncu çizgi çek
        Color debugColor = isBlue ? Color.blue : new Color(1f, 0.5f, 0f);
        Debug.DrawRay(origin, finalDirection * rayDistance, debugColor, 0.2f);

        if (hit.collider != null)
        {
            // BURADA DEÐÝÞÝKLÝK YAPTIK:
            // fireBlueNext yerine direkt gelen emre göre (isBlue) iþlem yapýyoruz.
            if (isBlue)
            {
                Debug.Log(">> Mavi Portal Atýldý (E) <<");
                SpawnBluePortal(hit.point, hit.normal);
            }
            else
            {
                Debug.Log(">> Turuncu Portal Atýldý (Q) <<");
                SpawnOrangePortal(hit.point, hit.normal);
            }

            // fireBlueNext satýrýný sildik çünkü artýk manuel seçim yapýyoruz.
        }
    }
    void DrawLaser(Vector2 startPos, Vector2 endPos, Color color)
    {
        if (lineRenderer == null) return;

        // Eðer bir önceki lazerden kalan kapatma sayacý varsa durdur
        if (laserCoroutine != null) StopCoroutine(laserCoroutine);

        lineRenderer.enabled = true;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.SetPosition(0, startPos); // Baþlangýç
        lineRenderer.SetPosition(1, endPos);   // Bitiþ

        // Belirlenen süre sonra kapatmak için sayacý baþlat
        laserCoroutine = StartCoroutine(DisableLaserAfterTime());
    }

    IEnumerator DisableLaserAfterTime()
    {
        yield return new WaitForSeconds(laserDuration);
        lineRenderer.enabled = false;
    }


    // --- AÞAÐIDAKÝLER AYNEN KALDI ---

    void SpawnBluePortal(Vector2 position, Vector2 normal)
    {
        if (currentBluePortal != null) Destroy(currentBluePortal);
        currentBluePortal = InstantiatePortal(bluePortalPrefab, position, normal);
        UpdatePortalLinks();
    }

    void SpawnOrangePortal(Vector2 position, Vector2 normal)
    {
        if (currentOrangePortal != null) Destroy(currentOrangePortal);
        currentOrangePortal = InstantiatePortal(orangePortalPrefab, position, normal);
        UpdatePortalLinks();
    }

    GameObject InstantiatePortal(GameObject prefab, Vector2 pos, Vector2 normal)
    {
        Vector2 spawnPos = pos + (normal * 0.1f);
        Quaternion baseRot = Quaternion.FromToRotation(Vector2.up, normal);
        Quaternion finalRot = baseRot * Quaternion.Euler(0, 0, rotationOffset);

        return Instantiate(prefab, spawnPos, finalRot);
    }

    void UpdatePortalLinks()
    {
        if (currentBluePortal != null && currentOrangePortal != null)
        {
            // Not: Portal scriptinin adýnýn 'Portal' olduðundan emin ol
            Portal blueScript = currentBluePortal.GetComponent<Portal>();
            Portal orangeScript = currentOrangePortal.GetComponent<Portal>();

            if (blueScript != null && orangeScript != null)
            {
                blueScript.linkedPortal = orangeScript;
                orangeScript.linkedPortal = blueScript;
            }
        }
    }

    Vector2 GetCardinalDirection(Vector2 direction)
    {
        float threshold = 0.1f;
        if (Mathf.Abs(direction.x) > threshold)
        {
            return new Vector2(Mathf.Sign(direction.x), 0);
        }
        else
        {
            if (Mathf.Abs(direction.y) > threshold)
                return new Vector2(0, Mathf.Sign(direction.y));
            else
                return playerMovement.FacingDirection;
        }
    }
}