using UnityEngine;

public class DosyaRaycast2D : MonoBehaviour
{
    [Header("Raycast Ayarlarý")]
    public float rayDistance = 1.5f;
    public LayerMask dosyaLayer; // Raycast'in çarpacaðý Layer

    [Header("E Basýlý Tut Ayarlarý")]
    public float holdTime = 0.1f;
    private float holdTimer = 0f;



    void Update()
    {
        Vector2 direction = PlayerMovement.FacingDirection; // Karakterin baktýðý yön
        Vector2 origin = (Vector2)transform.position + direction * 0.2f;

        // Raycast
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayDistance, dosyaLayer);
        Debug.DrawRay(origin, direction * rayDistance, Color.green);

        if (hit.collider != null)
        {
            GameObject parent = hit.collider.gameObject;

            int masaLayer = LayerMask.NameToLayer("Masa");
            int buyukMasaLayer = LayerMask.NameToLayer("BüyükMasa");

            // Anlýk E basma (Masa)
            if (Input.GetKeyDown(KeyCode.E) && parent.layer == masaLayer)
            {
                DestroyChildren(parent);
            }

            // 5 saniye hold (BüyükMasa)
            else if (parent.layer == buyukMasaLayer)
            {
                if (Input.GetKey(KeyCode.E))
                {
                    holdTimer += Time.deltaTime;

                    if (holdTimer >= holdTime)
                    {
                        DestroyChildren(parent);
                        holdTimer = 0f; // tekrar tetiklenmemesi için
                    }
                }
                else
                {
                    holdTimer = 0f; // tuþ býrakýlýrsa timer sýfýrlansýn
                }
            }
            else
            {
                // Baþka bir objeye bakýyorsa timer sýfýrlansýn
                holdTimer = 0f;
            }
        }
        else
        {
            // Hiçbir objeye bakmýyorsa timer sýfýrlansýn
            holdTimer = 0f;
        }
    }

    // Çocuklarý yok eden fonksiyon
    private void DestroyChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            if (!child.gameObject.CompareTag("Sabit")) // Sabit child'lar silinmez
            {
                Destroy(child.gameObject);
            }
        }
    }
}
