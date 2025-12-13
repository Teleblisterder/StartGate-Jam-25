using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public SkillBase[] skillSlots = new SkillBase[3];
    private int selectedSlotIndex = 0;

    // Uzun basýþla DISCARD mekaniði kaldýrýldýðý için qHoldTime ve DISCARD_HOLD_DURATION 
    // deðiþkenlerini koruyabilir veya kaldýrabilirsiniz. 
    // Bu versiyonda, karýþýklýðý önlemek için kullanýlmayanlarý kaldýrdým/yorumladým.

    // private float qHoldTime = 0f;
    // private const float DISCARD_HOLD_DURATION = 1.5f; 

    // SABÝT KONTEYNER ADI: Sizin sahnedeki adýnýz "Skills" ise bunu kullanýn.
    private const string SKILL_CONTAINER_NAME = "Skills";

    void Update()
    {
        // --- 1. SLOT SEÇÝMÝ ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);

        // --- Q TUÞU (ÝKÝNCÝL YETENEK) ---
        // Uzun basýþ (Discard) mekanizmasý kaldýrýldý.
        // Q tuþuna basýldýðý an yetenek kullanýlýr.
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (skillSlots[selectedSlotIndex] != null)
            {
                TryUseSkill(skillSlots[selectedSlotIndex], true); // true = UseSecondary (Q)
            }
        }

        // --- E TUÞU (BÝRÝNCÝL YETENEK) ---
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (skillSlots[selectedSlotIndex] != null)
            {
                TryUseSkill(skillSlots[selectedSlotIndex], false); // false = UsePrimary (E)
            }
        }
    }

    void SelectSlot(int index)
    {
        selectedSlotIndex = index;
        Debug.Log("Seçilen Slot: " + (index + 1));
    }

    private int FindNextEmptySlot(int startSlotIndex)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            int checkIndex = (startSlotIndex + i) % skillSlots.Length;

            if (skillSlots[checkIndex] == null)
            {
                return checkIndex; // Boþ slot bulundu
            }
        }
        return -1; // Boþ slot yok
    }

    public void EquipSkillToCurrentSlot(SkillBase newSkill)
    {
        int targetSlotIndex = selectedSlotIndex;

        if (skillSlots[selectedSlotIndex] != null)
        {
            int emptyIndex = FindNextEmptySlot(selectedSlotIndex);

            if (emptyIndex != -1)
            {
                targetSlotIndex = emptyIndex;
                Debug.LogWarning($"Slot {selectedSlotIndex + 1} dolu. Yetenek otomatik olarak boþ olan {emptyIndex + 1}. slota yerleþtirildi.");
            }
            else
            {
                // Tüm slotlar doluysa geri iade et
                Debug.LogWarning("Tüm slotlar dolu. Yetenek alýnamadý ve yerine geri döndü.");

                GameObject skillObj = newSkill.gameObject;
                GameObject container = GameObject.Find(SKILL_CONTAINER_NAME);

                if (container != null)
                {
                    skillObj.transform.SetParent(container.transform);
                    skillObj.gameObject.SetActive(false);
                }

                return;
            }
        }

        EquipSkill(newSkill, targetSlotIndex);
        selectedSlotIndex = targetSlotIndex;
    }

    public void EquipSkill(SkillBase newSkill, int slotIndex)
    {
        newSkill.transform.SetParent(this.transform);
        newSkill.transform.localPosition = Vector3.zero;
        newSkill.transform.localRotation = Quaternion.identity;
        newSkill.gameObject.SetActive(true);

        skillSlots[slotIndex] = newSkill;

        Debug.Log(newSkill.skillName + " alýndý ve " + (slotIndex + 1) + ". slota takýldý.");
    }

    public void DiscardCurrentSkill()
    {
        if (skillSlots[selectedSlotIndex] == null)
        {
            Debug.Log("Seçili slota takýlý yetenek yok.");
            return;
        }

        SkillBase discardedSkill = skillSlots[selectedSlotIndex];
        string skillName = discardedSkill.skillName;

        // 1. Slotu temizle
        skillSlots[selectedSlotIndex] = null;

        // 2. Yetenek objesinin kendisini tamamen YOK ET!
        Destroy(discardedSkill.gameObject, 1f);

        // Not: Q basýlý tutma süresi artýk kullanýlmadýðý için sýfýrlanmasýna gerek yok.
        // qHoldTime = 0f; 

        Debug.Log($"[{selectedSlotIndex + 1}. SLOTTAN ÇIKARILDI] {skillName} yeteneði tamamen yok edildi.");
    }

    private void TryUseSkill(SkillBase skill, bool isSecondary)
    {
        // 1. Can Kontrolü: Cooldown kontrolünü yeteneðin kendi içindeki UsePrimary/Secondary metoduna devrediyoruz.
        bool canConsume = skill.ConsumeDurability();

        if (canConsume)
        {
            // 2. Can var, yeteneði kullan
            if (isSecondary)
            {
                skill.UseSecondary();
            }
            else
            {
                skill.UsePrimary();
            }
        }

        // 3. Kullaným sonrasý can 0'a düþtüyse otomatik discard et
        if (skill.currentDurability <= 0)
        {
            Debug.LogWarning($"{skill.skillName} dayanýklýlýðý 0 oldu. Otomatik olarak slottan çýkarýlýyor!");
            DiscardCurrentSkill();
        }
    }
}