
using UnityEngine;

public class Telekinesi : SkillBase
{
    [Header("Referanslar")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform firePoint;

    [Header("Ayarlar")]
    public float pushForce = 20f; // Impulse çok güçlüdür, 100 yerine 20-30 dene
    public float pullForce = 20f;
    public float range = 50f;
    public string targetTag;    // "Kutu" vs. yazmayý unutma
    private void Start()
    {
        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();


        // 2. FirePoint Transformunu Bulma:
        // FirePoint'in Player objesinin bir alt objesi olduðunu varsayýyoruz.
        if (firePoint == null)
        {
            Transform playerParent = transform.parent;
            firePoint = playerParent.Find("FirePoint");

        }
    }

    // --- E TUÞU: ÝTME (PUSH) ---
    public override void UsePrimary()
    {
        // Cooldown kontrolü
        if (!IsReady())
        {
            // Eðer IsReady false döndürürse, yukarýdaki SkillBase metodundan uyarý logu zaten çýkar.
            return; // Hýzlýca çýkýþ yap.
        }

        Debug.Log(">> Telekinezi: ÝTME (E) <<");
        TryApplyForce(true); // true = Ýtme

        // Cooldown sýfýrla
        nextFireTime = Time.time + cooldownTime;
    }

    // --- Q TUÞU: ÇEKME (PULL) ---
    public override void UseSecondary()
    {
        // Cooldown kontrolü
        if (!IsReady())
        {
            // Eðer IsReady false döndürürse, yukarýdaki SkillBase metodundan uyarý logu zaten çýkar.
            return; // Hýzlýca çýkýþ yap.
        }

        Debug.Log(">> Telekinezi: ÇEKME (Q) <<");
        TryApplyForce(false); // false = Çekme

        // Cooldown sýfýrla
        nextFireTime = Time.time + cooldownTime;
    }

    // Mantýk fonksiyonu (Burada bir deðiþiklik yapmadým, sadece temizledim)
    void TryApplyForce(bool isPushing)
    {
        if (playerMovement == null) return;

        Vector2 rawDirection = playerMovement.FacingDirection;
        Vector2 finalDirection = GetCardinalDirection(rawDirection);

        // Iþýn karakterin biraz önünden çýksýn ki kendi colliderýna çarpmasýn
        Vector2 origin = (firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position) + finalDirection * 1f;

        RaycastHit2D hit = Physics2D.Raycast(origin, finalDirection, range);

        // Ýtme ise Kýrmýzý, Çekme ise Yeþil çizgi çiz
        Debug.DrawRay(origin, finalDirection * range, isPushing ? Color.red : Color.green, 0.5f);

        if (hit.collider != null)
        {
            // Eðer targetTag boþ deðilse ve çarptýðýmýz þeyin tag'i tutmuyorsa dur.
            if (!string.IsNullOrEmpty(targetTag) && !hit.collider.CompareTag(targetTag))
                return;

            Rigidbody2D boxRb = hit.collider.GetComponent<Rigidbody2D>();

            if (boxRb != null)
            {
                // Ýtme ise yön ayný, çekme ise yön ters (-finalDirection)
                Vector2 forceDirection = isPushing ? finalDirection : -finalDirection;
                float power = isPushing ? pushForce : pullForce;

                boxRb.AddForce(forceDirection * power, ForceMode2D.Impulse);
            }
        }
    }

    Vector2 GetCardinalDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return new Vector2(Mathf.Sign(direction.x), 0);
        }
        else
        {
            return new Vector2(0, Mathf.Sign(direction.y));
        }
    }
}