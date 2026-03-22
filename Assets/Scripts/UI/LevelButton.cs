using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 关卡按钮组件
/// 处理关卡的锁定/解锁、星级显示、点击事件
/// </summary>
public class LevelButton : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Text levelNumberText;
    [SerializeField] private Image levelImage;
    [SerializeField] private Image lockImage;
    [SerializeField] private GameObject[] starImages; // 0,1,2 三颗星
    [SerializeField] private Image progressRing; // 过关进度环

    [Header("状态颜色")]
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color completedColor = new Color(1f, 0.9f, 0.6f);

    private Button button;
    private int levelId;
    private bool isUnlocked;
    private int stars;
    private System.Action<int> onClickCallback;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    /// <summary>
    /// 初始化关卡按钮
    /// </summary>
    public void Init(int levelId, bool unlocked, int stars, System.Action<int> onClick)
    {
        this.levelId = levelId;
        this.isUnlocked = unlocked;
        this.stars = stars;
        this.onClickCallback = onClick;

        UpdateDisplay();

        if (button != null && isUnlocked)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    /// <summary>
    /// 更新按钮显示
    /// </summary>
    private void UpdateDisplay()
    {
        // 关卡编号
        if (levelNumberText != null)
        {
            levelNumberText.text = levelId.ToString();
        }

        // 锁定/解锁状态
        if (lockImage != null)
        {
            lockImage.gameObject.SetActive(!isUnlocked);
        }

        if (levelImage != null)
        {
            levelImage.color = isUnlocked ? (stars > 0 ? completedColor : unlockedColor) : lockedColor;
        }

        // 星级显示
        UpdateStars();

        // 按钮交互
        if (button != null)
        {
            button.interactable = isUnlocked;
        }
    }

    /// <summary>
    /// 更新星级显示
    /// </summary>
    private void UpdateStars()
    {
        if (starImages == null) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                starImages[i].gameObject.SetActive(isUnlocked && i < stars);
            }
        }
    }

    /// <summary>
    /// 设置星级
    /// </summary>
    public void SetStars(int newStars)
    {
        stars = newStars;
        UpdateStars();

        // 如果是新通关，更新颜色
        if (stars > 0 && levelImage != null)
        {
            levelImage.color = completedColor;
        }
    }

    /// <summary>
    /// 解锁关卡
    /// </summary>
    public void Unlock()
    {
        isUnlocked = true;
        UpdateDisplay();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        if (isUnlocked && onClickCallback != null)
        {
            onClickCallback.Invoke(levelId);
        }
    }

    /// <summary>
    /// 获取关卡ID
    /// </summary>
    public int GetLevelId()
    {
        return levelId;
    }

    /// <summary>
    /// 是否已解锁
    /// </summary>
    public bool IsUnlocked()
    {
        return isUnlocked;
    }
}
