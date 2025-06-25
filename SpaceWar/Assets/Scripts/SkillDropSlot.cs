using System.Collections;
//using UnityEditor.Localization.Platform.Android;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillDropSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private GameObject[] clonePrefabs; // Sırası: 0 - StarSparrow1, 1 - StarSparrow2, ...

    public static SkillDropSlot Instance;
    public SkillDetails skillData;
    public Image slotImage; // G�rsel g�sterilecek alan (iste�e ba�l�)
    public Button assignedSkillButton; // Oyuncunun t�klay�p kullanaca�� buton
    public GameObject SkillBUttonImage;
    public float cooldownTime;
    private Slider cooldownSlider;
    private bool isCoolingDown = false;


    private GameObject currentSkill;
    private GameObject playerShip;
    public GameObject shieldObject;
    public GameObject skill5Prefab;
    public GameObject skill7Prefab;
    public GameObject skill8Prefab;
    public GameObject skill11Prefab;

    public float explosionRadiusSkill12 = 15f;
    public GameObject skill12VFX;
    public Transform skillsParent; // Inspector'dan atanacak, skill objelerinin parent'ı
    public int slotIndex; // Inspector'dan atanabilir



    void Awake()=>Instance = this;
    void Start()
    {
        LoadSavedSkill();

        playerShip = GameObject.FindGameObjectWithTag("Player");
        shieldObject = playerShip.transform.Find("Sphere").gameObject;
        if (assignedSkillButton != null)
        {

            // Child Slider'� bul
            cooldownSlider = assignedSkillButton.GetComponentInChildren<Slider>();
            if (cooldownSlider != null)
            {
                cooldownSlider.maxValue = cooldownTime;
                cooldownSlider.value = cooldownTime; // Ba�ta dolu g�r�n�r
            }

        }
    }
    void Update()
    {
        if (playerShip == null)
        {
            playerShip = GameObject.FindGameObjectWithTag("Player");
            if (playerShip == null) return; // Hâlâ yoksa fonksiyondan çık
        }
        if (shieldObject == null)
        {
            shieldObject = playerShip.transform.Find("Sphere").gameObject;
            if (shieldObject == null) return; // Hâlâ yoksa fonksiyondan çık
        }
    }
    void LoadSavedSkill()
    {
        string savedSkillName = PlayerPrefs.GetString("SavedSkill_Slot" + slotIndex, "");
        if (!string.IsNullOrEmpty(savedSkillName))
        {
            // Hierarchy'deki skill objesi (hepsi aynı parent altında olmalı)
            Transform skillTransform = skillsParent.Find(savedSkillName);
            if (skillTransform != null)
            {
                // Instantiate edip slot içine koy
                GameObject skillClone = Instantiate(skillTransform.gameObject, transform);
                skillClone.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                Destroy(skillClone.GetComponent<SkillDragHandler>());
                var group = skillClone.GetComponent<CanvasGroup>();
                if (group != null)
                    group.blocksRaycasts = true;

                currentSkill = skillClone;

                // Skill slot butonunu aktif et, görsel ayarla, listener ekle
                if (assignedSkillButton != null)
                {
                    assignedSkillButton.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    SkillBUttonImage.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    assignedSkillButton.onClick.RemoveAllListeners();
                    assignedSkillButton.onClick.AddListener(() => TryUseSkill(skillClone.name));
                    assignedSkillButton.interactable = true;

                    if (cooldownSlider != null)
                    {
                        cooldownSlider.maxValue = cooldownTime;
                        cooldownSlider.value = cooldownTime;
                    }
                }

                if (slotImage != null)
                {
                    Image skillImage = skillClone.GetComponent<Image>();
                    if (skillImage != null)
                    {
                        slotImage.sprite = skillImage.sprite;
                        slotImage.color = Color.white;
                        assignedSkillButton.GetComponent<Image>().sprite = slotImage.sprite;
                    }
                }
            }
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        SkillDragHandler skill = dropped.GetComponent<SkillDragHandler>();
        if (skill != null && skill.isUnlocked)
        {
            // Eski skill varsa sil
            if (currentSkill != null)
            {
                Destroy(currentSkill);
            }

            // Skill kopyas� olu�tur (drop olan objeyi yok ediyoruz zaten)
            GameObject skillClone = Instantiate(dropped, transform);
            skillClone.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            // Drag edilemesin art�k
            Destroy(skillClone.GetComponent<SkillDragHandler>());
            var group = skillClone.GetComponent<CanvasGroup>();
            if (group != null)
                group.blocksRaycasts = true;

            currentSkill = skillClone;
            // currentSkill objesinin ismini kaydet (clone kısmını çıkar)
            string skillName = skillClone.name.Replace("(Clone)", "");
            PlayerPrefs.SetString("SavedSkill_Slot" + slotIndex, skillName);
            PlayerPrefs.Save();


            // Skill slot butonunu aktif et
            if (assignedSkillButton != null)
            {
                assignedSkillButton.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                SkillBUttonImage.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                assignedSkillButton.onClick.RemoveAllListeners();
                assignedSkillButton.onClick.AddListener(() => TryUseSkill(skillClone.name));
                assignedSkillButton.interactable = true;

                if (cooldownSlider != null)
                {
                    cooldownSlider.maxValue = cooldownTime;
                    cooldownSlider.value = cooldownTime;
                }
            }

            // Slot g�rselini de�i�tir (iste�e ba�l�)
            if (slotImage != null)
            {
                Image skillImage = skillClone.GetComponent<Image>();
                if (skillImage != null)
                {
                    slotImage.sprite = skillImage.sprite;
                    slotImage.color = Color.white;
                    assignedSkillButton.GetComponent<Image>().sprite = slotImage.sprite;

                }
            }
        }
    }
    private void TryUseSkill(string skillName)
    {
        if (!isCoolingDown)
        {
            UseSkill(skillName);
            StartCoroutine(CooldownRoutine());
        }
    }
    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;
        assignedSkillButton.interactable = false;

        float timer = 0f;
        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;
            if (cooldownSlider != null)
                cooldownSlider.value = timer;

            yield return null;
        }

        if (cooldownSlider != null)
            cooldownSlider.value = cooldownTime;

        assignedSkillButton.interactable = true;
        isCoolingDown = false;
    }

    private void UseSkill(string skillName)
    {
        switch (skillName)
        {
            case "Skill1(Clone)": DashForward(100f,0.35f);Debug.Log($"Skill kullanıldı: {skillName}"); cooldownTime = 4f; cooldownSlider.maxValue = cooldownTime; break;

            case "Skill2(Clone)": PlayerSmoothFollow.Instance.StartHealthRegenOverTime(); Debug.Log($"Skill kullan�ld�: {skillName}"); cooldownTime = 5f; cooldownSlider.maxValue = cooldownTime; break;


            case "Skill3(Clone)": ActivateShield(3); Debug.Log($"Skill kullan�ld�: {skillName}"); cooldownTime = 7f; cooldownSlider.maxValue = cooldownTime; break;


            case "Skill4(Clone)": StartCoroutine(InvisibilityRoutine(4)); Debug.Log($"Skill kullan�ld�: {skillName}"); cooldownTime = 8f; cooldownSlider.maxValue = cooldownTime; break;


            case "Skill5(Clone)": SpawnPrefabBehindPlayer(); Debug.Log($"Skill kullan�ld�: {skillName}"); cooldownTime = 14f; cooldownSlider.maxValue = cooldownTime; break;


            case "Skill6(Clone)": EnemeyBulletOff(4f); Debug.Log($"Skill kullan�ld�: {skillName}"); cooldownTime = 10f; cooldownSlider.maxValue = cooldownTime; break;


            case "Skill7(Clone)": DropPrefabOnClosestEnemy(); Debug.Log($"Skill kullanıldı: {skillName}"); cooldownTime = 15f; cooldownSlider.maxValue = cooldownTime; break;


            case "Skill8(Clone)": SpawnClone(); Debug.Log($"Skill kullan�ld�: {skillName}"); cooldownTime = 17f; cooldownSlider.maxValue = cooldownTime; break;


            case "Skill9(Clone)": EnemeyMoveOff(3f); Debug.Log($"Skill kullan�ld�: {skillName}"); cooldownTime = 15f; cooldownSlider.maxValue = cooldownTime; break;


            case "Skill10(Clone)": PlayerSmoothFollow.Instance.UseSkill10(); Debug.Log($"Skill kullan�ld�: {skillName}"); cooldownTime = 20f; cooldownSlider.maxValue = cooldownTime; break;


            case "Skill11(Clone)": ActivateSkill11(); Debug.Log($"Skill kullan�ld�: {skillName}"); cooldownTime = 20f; cooldownSlider.maxValue = cooldownTime; break;


            case "Skill12(Clone)": NuklearBomb(); Debug.Log($"Skill kullan�ld�: {skillName}"); cooldownTime = 20f; cooldownSlider.maxValue = cooldownTime; break;


            default: Debug.Log("Bos"); break;

        }
    }


/// Player'in ileriye dashing (skill1)
    private void DashForward(float dashForce = 20f, float dashDuration = 0.2f)
    {
        if (playerShip == null) return;

        Rigidbody rb = playerShip.GetComponent<Rigidbody>();
        if (rb != null)
        {
            PlayerSmoothFollow.Instance.isDashing = true;
            Vector3 dashDirection = playerShip.transform.forward.normalized;
            rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
            StartCoroutine(StopDashAfterTime(rb, dashDuration));
        }
    }
    private IEnumerator StopDashAfterTime(Rigidbody rb, float duration)
    {
        yield return new WaitForSeconds(duration);
        rb.linearVelocity = Vector3.zero; // Hareketi durdur
        PlayerSmoothFollow.Instance.isDashing = false;
    }

/// Shieldi aktif hale getir (skill3)
    public void ActivateShield(float duration)
    {
        shieldObject.SetActive(true);
        StartCoroutine(DeactivateShieldAfterSeconds(duration));
    }
    private IEnumerator DeactivateShieldAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        shieldObject.SetActive(false);
    }

/// Görünmez olur (skill4)
    private IEnumerator InvisibilityRoutine(float invisibilityDuration)
    {
        PlayerSmoothFollow.Instance.isInvisible = true;

        Renderer[] renderers = playerShip.GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                SetMaterialTransparent(mat, 0.02f); // %20 görünürlük
            }
        }

        yield return new WaitForSeconds(invisibilityDuration);

        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                SetMaterialOpaque(mat); // Eski haline döndür
            }
        }

        PlayerSmoothFollow.Instance.isInvisible = false;
    }
    private void SetMaterialTransparent(Material mat, float alpha)
    {
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        Color color = mat.color;
        color.a = alpha;
        mat.color = color;
    }
    private void SetMaterialOpaque(Material mat)
    {
        mat.SetFloat("_Mode", 0); // Opaque
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = -1;

        Color color = mat.color;
        color.a = 1f;
        mat.color = color;
    }

/// Turret spawnlar (skill5)
    private void SpawnPrefabBehindPlayer()
    {
        if (playerShip == null || skill5Prefab == null)
        {
            Debug.LogWarning("PlayerShip ya da Prefab eksik.");
            return;
        }

        Vector3 spawnPosition = playerShip.transform.position + playerShip.transform.forward * 5f;

        Instantiate(skill5Prefab, spawnPosition, Quaternion.identity);
    }

/// Enemy'in bullet'larını kapat (skill6)
    public void EnemeyBulletOff(float duration)
    {
        PlayerSmoothFollow.Instance.enemyBulletOff = true;
        StartCoroutine(EnemyBulletOffDuration(duration));
    }
    private IEnumerator EnemyBulletOffDuration(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        PlayerSmoothFollow.Instance.enemyBulletOff = false;
    }

/// Meteror yağdır (skill7)
    private void DropPrefabOnClosestEnemy()
    {
        GameObject closestEnemy = FindClosestEnemy();
        if (closestEnemy == null || skill7Prefab == null)
        {
            Debug.LogWarning("Düşman ya da prefab eksik.");
            return;
        }

        Vector3 spawnPosition = closestEnemy.transform.position + Vector3.up * 10f; // Düşmanın 10 birim üstü
        GameObject fallingObject = Instantiate(skill7Prefab, spawnPosition, Quaternion.identity);

        Rigidbody rb = fallingObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = fallingObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = true;
        rb.mass = 1f;
    }
    private GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0) return null;

        GameObject closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 playerPos = playerShip.transform.position;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(playerPos, enemy.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = enemy;
            }
        }

        return closest;
    }

/// Clone oluştur (skill8)
    void SpawnClone()
    {
        if (playerShip == null)
        {
            Debug.LogWarning("PlayerShip atanmadı!");
            return;
        }

        string originalName = playerShip.name.Replace("(Clone)", "").Trim();

        GameObject selectedPrefab = null;

        foreach (GameObject prefab in clonePrefabs)
        {
            if (prefab != null && prefab.name == originalName)
            {
                selectedPrefab = prefab;
                break;
            }
        }

        if (selectedPrefab == null)
        {
            Debug.LogWarning($"'{originalName}' ismine sahip bir prefab bulunamadı!");
            return;
        }

        Vector3 flatForward = new Vector3(playerShip.transform.forward.x, 0f, playerShip.transform.forward.z).normalized;
        Vector3 spawnPos = playerShip.transform.position - flatForward * 2f;
        spawnPos.y = playerShip.transform.position.y;

        Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
    }


/// Enemy'in hareketini kapat (skill9)
    public void EnemeyMoveOff(float duration)
    {
        PlayerSmoothFollow.Instance.enemyMoveOff = true;
        StartCoroutine(EnemyMoveOfff(duration));
    }
    private IEnumerator EnemyMoveOfff(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        PlayerSmoothFollow.Instance.enemyMoveOff = false;
    }


/// Karadelik oluşturur (skill11)
    public void ActivateSkill11()
    {
        Vector3 flatForward = new Vector3(playerShip.transform.forward.x, 0f, playerShip.transform.forward.z).normalized;

        Vector3 spawnPosition = playerShip.transform.position + flatForward * 15f + Vector3.down * 7f; ;  // Player'ın 10 birim önü
        GameObject pullField = Instantiate(skill11Prefab, spawnPosition, Quaternion.identity);
    }

/// Nükleer Patlama (skill12)
    public void NuklearBomb()
    {
        // Patlama efekti varsa göster
        if (skill12VFX != null)
        {
            Instantiate(skill12VFX, playerShip.transform.position, Quaternion.identity);
        }

        // Çevredeki tüm enemy objelerini bul
        Collider[] hitColliders = Physics.OverlapSphere(playerShip.transform.position, explosionRadiusSkill12);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(PlayerSmoothFollow.Instance.damage * 10);
                }
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadiusSkill12);
    }
}
