using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 道具按钮组件
/// </summary>
public class ItemButton : MonoBehaviour, IPointerClickHandler
{
    [Header("UI组件")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text quantityText;
    [SerializeField] private Text priceText;
    [SerializeField] private Button buyButton;

    [Header("状态")]
    [SerializeField] private bool isOwned = false;  // 是否已拥有

    private ItemData itemData;
    private System.Action<ItemButton> onClickCallback;

    public ItemData ItemData => itemData;

    /// <summary>
    /// 初始化道具按钮
    /// </summary>
    public void Init(ItemData data, bool owned, System.Action<ItemButton> callback = null)
    {
        itemData = data;
        isOwned = owned;
        onClickCallback = callback;

        UpdateDisplay();
    }

    /// <summary>
    /// 更新显示
    /// </summary>
    private void UpdateDisplay()
    {
        if (itemData == null) return;

        // 设置图标
        if (iconImage != null)
        {
            iconImage.sprite = itemData.icon;
        }

        // 设置数量
        if (quantityText != null)
        {
            quantityText.text = isOwned ? itemData.quantity.ToString() : "";
            quantityText.gameObject.SetActive(isOwned);
        }

        // 设置价格
        if (priceText != null)
        {
            priceText.text = isOwned ? "" : itemData.price.ToString();
            priceText.gameObject.SetActive(!isOwned);
        }

        // 设置购买按钮状态
        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(!isOwned);

            if (!isOwned)
            {
                // 检查是否买得起
                bool canAfford = GameManager.Instance.Coins >= itemData.price;
                buyButton.interactable = canAfford;
            }
        }
    }

    /// <summary>
    /// 使用道具
    /// </summary>
    public void Use()
    {
        if (!isOwned || itemData.quantity <= 0) return;

        // 调用道具系统使用
        if (ItemSystem.Instance.UseItem(itemData.itemId))
        {
            UpdateDisplay();
        }
    }

    /// <summary>
    /// 购买道具
    /// </summary>
    public void Buy()
    {
        if (isOwned) return;

        if (ItemSystem.Instance.PurchaseItem(itemData.itemId))
        {
            isOwned = true;
            UpdateDisplay();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(this);
    }
}
