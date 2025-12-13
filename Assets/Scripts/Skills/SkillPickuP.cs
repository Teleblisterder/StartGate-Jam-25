using UnityEngine;


public class SkillPickup : MonoBehaviour
{
    [Header("Yaratýlacak Yetenek Prefabý")]
    [Tooltip("Project klasöründeki yetenek Prefab'ýný (örn: Skill_Portal) buraya sürükleyin.")]
    // DÝKKAT: Artýk Hierarchy'deki objeyi deðil, Project'teki prefabý istiyoruz.
    public GameObject skillPrefab;

    // 2D Trigger'ýmýz
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSkillManager manager = other.GetComponent<PlayerSkillManager>();

            if (manager != null)
            {
                PickUp(manager);
            }
        }
    }

    void PickUp(PlayerSkillManager manager)
    {
        if (skillPrefab == null)
        {
            // Artýk transfer edilecek objemiz deðil, Prefab'ýmýz eksik olabilir.
            Debug.LogError("SkillPickup hatasý: Yetenek Prefab'ý atanmamýþ! Lütfen Project klasöründen Prefab'ý sürükleyin.");
            return;
        }

        // --- KRÝTÝK DEÐÝÞÝKLÝK: YENÝ BÝR KOPYA OLUÞTURUYORUZ ---
        GameObject newSkillObj = Instantiate(skillPrefab);

        // Yeteneðin SkillBase komponentini al
        SkillBase skillComponent = newSkillObj.GetComponent<SkillBase>();

        if (skillComponent != null)
        {
            // Yeni oluþturulan kopyayý Manager'a gönder.
            // Manager onu Player'ýn child'ý yapacak ve referanslarý kendi bulacak (OnEnable metodu ile).
            manager.EquipSkillToCurrentSlot(skillComponent);

            // Yerdeki kutuyu (yani bu scriptin bulunduðu objeyi) yok et
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Atanan Prefab'ýn üzerinde SkillBase (veya türevi) yok! Prefab doðru mu?");
            Destroy(newSkillObj); // Hatalý oluþturulan objeyi temizle
        }
    }
}