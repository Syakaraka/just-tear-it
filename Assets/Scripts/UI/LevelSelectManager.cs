using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 关卡选择管理器
/// </summary>
public class LevelSelectManager : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Transform levelGridParent;        // 关卡按钮父物体
    [SerializeField] private GameObject levelButtonPrefab;     // 关卡按钮预制体
    [SerializeField] private Button backButton;
    [SerializeField] private Text chapterTitleText;            // 章节标题
    [SerializeField] private Button prevChapterButton;
    [SerializeField] private Button nextChapterButton;
    [SerializeField] private Text totalStarsText;              // 总星级显示

    [Header("章节配置")]
    [SerializeField] private int totalChapters = 3;
    [SerializeField] private int levelsPerChapter = 5;
    [SerializeField] private string[] chapterNames = { "第一章 胶带篇", "第二章 贴纸篇", "第三章 包装篇" };

    [Header("场景配置")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainSceneName = "MainScene";

    private List<LevelButton> levelButtons = new List<LevelButton>();
    private int currentChapter = 1;

    private void Start()
    {
        // 设置游戏状态
        GameManager.Instance.SetGameState(GameState.LevelSelect);

        // 初始化UI
        InitUI();

        // 生成关卡按钮
        GenerateLevelButtons();

        // 更新总星级显示
        UpdateTotalStars();
    }

    /// <summary>
    /// 初始化UI事件
    /// </summary>
    private void InitUI()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (prevChapterButton != null)
            prevChapterButton.onClick.AddListener(() => SwitchChapter(currentChapter - 1));

        if (nextChapterButton != null)
            nextChapterButton.onClick.AddListener(() => SwitchChapter(currentChapter + 1));

        UpdateChapterTitle();
        UpdateChapterButtons();
    }

    /// <summary>
    /// 生成关卡按钮
    /// </summary>
    private void GenerateLevelButtons()
    {
        if (levelGridParent == null || levelButtonPrefab == null)
        {
            Debug.LogWarning("Level grid or prefab not assigned!");
            return;
        }

        // 清空现有按钮
        ClearLevelButtons();

        // 计算当前章节起始关卡
        int startLevel = (currentChapter - 1) * levelsPerChapter + 1;

        // 生成章节内所有关卡按钮
        for (int i = 0; i < levelsPerChapter; i++)
        {
            int levelId = startLevel + i;

            // 使用预制体或动态创建
            GameObject levelBtnObj;
            if (levelButtonPrefab != null)
            {
                levelBtnObj = Instantiate(levelButtonPrefab, levelGridParent);
            }
            else
            {
                levelBtnObj = CreateDefaultLevelButton(levelId);
                levelBtnObj.transform.SetParent(levelGridParent);
            }

            // 尝试获取LevelButton组件
            var levelButton = levelBtnObj.GetComponent<LevelButton>();
            if (levelButton != null)
            {
                bool isUnlocked = GameManager.Instance.IsLevelUnlocked(levelId);
                int stars = GameManager.Instance.GetLevelStars(levelId);
                levelButton.Init(levelId, isUnlocked, stars, OnLevelClicked);
                levelButtons.Add(levelButton);
            }
            else
            {
                // 如果没有LevelButton组件，使用简单的Button
                SetupSimpleButton(levelBtnObj, levelId);
            }
        }
    }

    /// <summary>
    /// 创建默认关卡按钮
    /// </summary>
    private GameObject CreateDefaultLevelButton(int levelId)
    {
        var go = new GameObject($"Level_{levelId}");

        // 添加必要的组件
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(150, 150);

        var image = go.AddComponent<Image>();
        image.color = Color.white;

        var btn = go.AddComponent<Button>();
        btn.transition = Selectable.Transition.ColorTint;

        var text = go.AddComponent<Text>();
        text.text = levelId.ToString();
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 40;
        text.color = Color.black;

        return go;
    }

    /// <summary>
    /// 设置简单按钮（无LevelButton组件时）
    /// </summary>
    private void SetupSimpleButton(GameObject levelBtnObj, int levelId)
    {
        var button = levelBtnObj.GetComponent<Button>();
        if (button != null)
        {
            bool isUnlocked = GameManager.Instance.IsLevelUnlocked(levelId);
            button.interactable = isUnlocked;

            if (isUnlocked)
            {
                button.onClick.AddListener(() => OnLevelClicked(levelId));
            }

            // 设置颜色
            var image = levelBtnObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = isUnlocked ? Color.white : Color.gray;
            }
        }
    }

    /// <summary>
    /// 清空关卡按钮
    /// </summary>
    private void ClearLevelButtons()
    {
        foreach (var btn in levelButtons)
        {
            if (btn != null)
            {
                Destroy(btn.gameObject);
            }
        }
        levelButtons.Clear();

        // 清空子物体（不包括预制体本身）
        foreach (Transform child in levelGridParent)
        {
            if (child.gameObject != levelButtonPrefab)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// 关卡按钮点击
    /// </summary>
    private void OnLevelClicked(int levelId)
    {
        Debug.Log($"Selected level: {levelId}");

        // 播放音效
        AudioManager.Instance?.PlayLevelSelect();

        // 记录开始关卡
        AnalyticsManager.Instance?.LogLevelStart(levelId);

        // 加载关卡
        GameManager.Instance.StartLevel(levelId);
        SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 返回按钮点击
    /// </summary>
    private void OnBackClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 切换章节
    /// </summary>
    public void SwitchChapter(int chapter)
    {
        if (chapter < 1 || chapter > totalChapters) return;

        AudioManager.Instance?.PlayButtonClick();

        currentChapter = chapter;
        GenerateLevelButtons();
        UpdateChapterTitle();
        UpdateChapterButtons();
    }

    /// <summary>
    /// 更新章节标题
    /// </summary>
    private void UpdateChapterTitle()
    {
        if (chapterTitleText != null && chapterNames != null && currentChapter <= chapterNames.Length)
        {
            chapterTitleText.text = chapterNames[currentChapter - 1];
        }
    }

    /// <summary>
    /// 更新章节切换按钮状态
    /// </summary>
    private void UpdateChapterButtons()
    {
        if (prevChapterButton != null)
            prevChapterButton.interactable = currentChapter > 1;

        if (nextChapterButton != null)
            nextChapterButton.interactable = currentChapter < totalChapters;
    }

    /// <summary>
    /// 更新总星级显示
    /// </summary>
    private void UpdateTotalStars()
    {
        if (totalStarsText == null) return;

        int totalStars = 0;
        int maxStars = GameManager.Instance.TotalLevels * 3; // 每关最高3星

        for (int i = 1; i <= GameManager.Instance.TotalLevels; i++)
        {
            totalStars += GameManager.Instance.GetLevelStars(i);
        }

        totalStarsText.text = $"{totalStars}/{maxStars}";
    }

    /// <summary>
    /// 刷新关卡显示（从结算界面返回时调用）
    /// </summary>
    public void RefreshLevelButtons()
    {
        GenerateLevelButtons();
        UpdateTotalStars();
    }
}
