using UnityEngine;

    
public abstract class SkillBase : MonoBehaviour
{
    public string skillName;
    public float cooldownTime = 1f;
    protected float nextFireTime = 0f;

    [Header("Dayanýklýlýk")]
    public int maxDurability = 4;
    [HideInInspector]
    public int currentDurability;

    public bool ConsumeDurability()
    {
        if (currentDurability > 0 && IsReady())
        {
            currentDurability--;
            Debug.Log($"[{skillName}] kullanýldý. Kalan Can: {currentDurability}");
            return true;
        }
        return false; // Can kalmadý, kullanýma izin verilmedi.
    }


    private void Awake()
    {
        currentDurability = maxDurability;
        nextFireTime = 0f;
    }
    protected bool IsReady()
    {
        bool ready = Time.time >= nextFireTime;

        // Eðer hazýr deðilse, ne kadar kaldýðýný loglayalým.
        if (!ready)
        {
            Debug.LogWarning($"[{skillName}]: Cooldown'da! Kalan süre: {nextFireTime - Time.time:F2}s");
            return false;
        }

        return true;
    }

    // YENÝ: Cooldown'ý baþarýlý kullanýmdan sonra baþlatalým
    protected void StartCooldown()
    {
        nextFireTime = Time.time + cooldownTime;
    }
    public virtual void UsePrimary()
    {
        // Boþ býrakýyoruz. Türeten sýnýf dolduracak.
    }

    public virtual void UseSecondary()
    {
        // Boþ. Eðer bir silahýn Q özelliði yoksa burasý çalýþýr (yani hiçbir þey yapmaz).
    }

    
}

