using UnityEngine;
using System.Collections; // IEnumerator için gerekli

public class Telekinesi : SkillBase
{
    [Header("Referanslar")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform firePoint;

    [Header("Görsel Efektler")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float laserDuration = 0.1f;
    private Coroutine laserCoroutine;

    [Header("Ayarlar")]
    public float pushForce = 20f;
    public float pullForce = 20f;
    public float range = 50f;
    public string targetTag;

    private void Start()
    {
        // 1. OYUNCUYU BUL (Garanti Yöntem)
        GameObject player = GameObject.FindGameObjectWithTag("Player");

            // PlayerMovement'ý al
            if (playerMovement == null)
                playerMovement = GetComponentInParent<PlayerMovement>();

            // 2. LINE RENDERER'I BUL (Player'ýn içindeki "LaserEffect" objesinden)
            if (lineRenderer == null)
            {
                Transform laserObj = player.transform.Find("LaserEffect");
                if (laserObj != null)
                {
                    lineRenderer = laserObj.GetComponent<LineRenderer>();
                    lineRenderer.enabled = false; // Baþlangýçta kapat
                }
                else
                {
                    Debug.LogError("HATA: Player'ýn içinde 'LaserEffect' adýnda bir obje yok!");
                }
            }

            // 3. FIREPOINT'I BUL
            if (firePoint == null)
            {
                firePoint = player.transform.Find("FirePoint");
            }
        else
        {
            Debug.LogError("HATA: Sahnede 'Player' tagine sahip bir obje bulunamadý!");
        }
    }

    // --- E TUÞU: ÝTME (PUSH) ---
    public override void UsePrimary()
    {
        if (!IsReady()) return;
        TryApplyForce(true); // true = Ýtme
        nextFireTime = Time.time + cooldownTime;
    }

    // --- Q TUÞU: ÇEKME (PULL) ---
    public override void UseSecondary()
    {
        if (!IsReady()) return;
        TryApplyForce(false); // false = Çekme
        nextFireTime = Time.time + cooldownTime;
    }

    void TryApplyForce(bool isPushing)
    {
        if (playerMovement == null) return;

        Vector2 rawDirection = playerMovement.FacingDirection;
        Vector2 finalDirection = GetCardinalDirection(rawDirection);

        // Iþýn karakterin biraz önünden çýksýn
        Vector2 origin = (firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position) + finalDirection * 1f;

        RaycastHit2D hit = Physics2D.Raycast(origin, finalDirection, range);

        // --- GÖRSEL KISIM: Lazer Rengini Belirle ve Çiz ---
        Color laserColor = isPushing ? Color.red : Color.green; // Ýtme Kýrmýzý, Çekme Yeþil

        // Eðer bir þeye çarptýysa oraya kadar, çarpmadýysa menzil sonuna kadar çiz
        Vector2 endPoint = hit.collider != null ? hit.point : (origin + finalDirection * range);

        DrawLaser(origin, endPoint, laserColor);
        // --------------------------------------------------

        if (hit.collider != null)
        {
            // Tag kontrolü (Eðer targetTag boþ deðilse kontrol et)
            if (!string.IsNullOrEmpty(targetTag) && !hit.collider.CompareTag(targetTag))
                return;

            Rigidbody2D boxRb = hit.collider.GetComponent<Rigidbody2D>();

            if (boxRb != null)
            {
                Vector2 forceDirection = isPushing ? finalDirection : -finalDirection;
                float power = isPushing ? pushForce : pullForce;

                // ForceMode2D.Impulse anlýk vuruþ hissi verir
                boxRb.AddForce(forceDirection * power, ForceMode2D.Impulse);
            }
        }
    }

    // --- LAZER ÇÝZME FONKSÝYONLARI ---
    void DrawLaser(Vector2 startPos, Vector2 endPos, Color color)
    {
        if (lineRenderer == null) return;

        if (laserCoroutine != null) StopCoroutine(laserCoroutine);

        lineRenderer.enabled = true;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        laserCoroutine = StartCoroutine(DisableLaserAfterTime());
    }

    IEnumerator DisableLaserAfterTime()
    {
        yield return new WaitForSeconds(laserDuration);
        lineRenderer.enabled = false;
    }

    Vector2 GetCardinalDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            return new Vector2(Mathf.Sign(direction.x), 0);
        else
            return new Vector2(0, Mathf.Sign(direction.y));
    }
}