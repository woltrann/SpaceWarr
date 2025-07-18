using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Bu script, skill butonlarýnýn sürüklenip býrakýlabilmesini saðlar.
/// Skill satýn alýndýysa (isUnlocked = true) kullanýcý bunu drag-drop ile slotlara koyabilir.
/// </summary>
public class SkillDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int skillIndex; // Skill'e özel ID (Inspector’dan ayarlanmalý)
    public Text priceText; // Inspector'dan atanacak
    public bool isUnlocked = false; // Bu skill satýn alýndý mý?

    private CanvasGroup canvasGroup;         // UI öðesinin raycast kontrolü için
    private RectTransform rectTransform;     // UI öðesinin konumunu kontrol etmek için
    private Transform originalParent;        // Drag iþlemi sýrasýnda geri dönebileceði orijinal parent
    private GameObject draggingIcon;         // Sürüklenen skill'in ekranda oluþan geçici kopyasý

  

    void Awake()
    {
        // Gerekli bileþenleri baþta alýyoruz
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
  void Start()
    {
        // KAYITLI VERÝYÝ KONTROL ET
        if (PlayerPrefs.GetInt("SkillUnlocked_" + skillIndex, 0) == 1)
        {
            UnlockSkill();
        }
    }
    public void OnBeginDrag(PointerEventData eventData) // Drag baþladýðýnda çalýþýr. Skill kopyasýný oluþturur ve sürüklenmeye hazýr hale getirir.
    {
        if (!isUnlocked) return; // Skill kilitliyse iþlem yapma

        draggingIcon = Instantiate(gameObject, transform.root);         // Skill butonunun bir kopyasýný oluþturuyoruz, böylece orijinal yerinde kalýr
        draggingIcon.transform.SetAsLastSibling(); 

        var group = draggingIcon.GetComponent<CanvasGroup>();        // Kopyanýn üzerinden raycast geçmesin, yani baþka UI öðeleriyle çakýþmasýn
        if (group != null)
            group.blocksRaycasts = false;

        originalParent = transform.parent;        // Orijinal parent'ý hatýrlýyoruz (geri döndürmek gerekirse diye)
    }

    public void OnDrag(PointerEventData eventData)  // Drag devam ederken çalýþýr. Kopya skill UI elemanýný fare konumuna taþýr.
    {
        if (!isUnlocked || draggingIcon == null) return;
        draggingIcon.GetComponent<RectTransform>().position = eventData.position;       // Kopya butonu farenin olduðu yere getiriyoruz
    }


    public void OnEndDrag(PointerEventData eventData)       // Drag býrakýldýðýnda çalýþýr. Býrakýlan alan bir drop alaný deðilse kopyayý yok eder.
    {
        if (!isUnlocked || draggingIcon == null) return;   
        var group = draggingIcon.GetComponent<CanvasGroup>();       // Artýk raycast'e açýk olabilir
        if (group != null)
            group.blocksRaycasts = true;

        Destroy(draggingIcon, 0.1f);         // Eðer geçerli bir drop alanýna býrakýlmazsa kopyayý sil
    }

    public void UnlockSkill()    // Skill satýn alýndýðýnda çaðrýlýr. Skill artýk kullanýlabilir (sürüklenebilir) olur.
    {
        isUnlocked = true; // Artýk drag yapýlabilir   
        GetComponent<Button>().interactable = false;        // Artýk butona týklanmasýn, çünkü iþlevi sadece drag olacak
        GetComponent<Image>().color = Color.white;        // Rengini beyaz yap, örneðin satýn alýnmýþ hissi vermek için
    }
}
