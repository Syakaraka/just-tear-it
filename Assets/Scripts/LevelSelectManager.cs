using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("章节配置")]
    [SerializeField] private int totalChapters = 3;
    [SerializeField] private int levelsPerChapter = 5;
    [SerializeField] private string[] chapterNames = { "第一章 胶带篇", "第二章 贴纸篇", "第三章 包装篇" };

    [Header("场景配置")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainSceneName = "MainScene";

    private List<GameObject> levelButtons = new List<GameObject>();
    private int currentChapter = 1;

    private void Start()
    {
        GameManager.Instance.SetGameState(GameState.LevelSelect);

        // 生成关卡按钮
        GenerateLevelButtons();

        // 绑定返回按钮
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        // 绑定章节切换按钮
        if (prevChapterButton != null)
            prevChapterButton.onClick.AddListener(() => SwitchChapter(currentChapter - 1));

        if (nextChapterButton != null)
            nextChapterButton.onClick.AddListener(() => SwitchChapter(currentChapter + 1));

        // 更新章节标题
        UpdateChapterTitle();
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
        foreach (var btn in levelButtons)
        {
            Destroy(btn);
        }
        levelButtons.Clear();

        // 计算当前章节起始关卡
        int startLevel = (currentChapter - 1) * levelsPerChapter + 1;

        // 生成章节内所有关卡按钮
        for (int i = 0; i < levelsPerChapter; i++)
        {
            int levelId = startLevel + i;
            var levelBtn = Instantiate(levelButtonPrefab, levelGridParent);

            // 设置关卡号
            var levelText = levelBtn.transform.Find("LevelText")?.GetComponent<Text>();
            if (levelText != null)
            {
                levelText.text = levelId.ToString();
            }

            // 设置按钮点击事件
            var button = levelBtn.GetComponent<Button>();
            if (button != null)
            {
                bool isUnlocked = GameManager.Instance.IsLevelUnlocked(levelId);
                button.interactable = isUnlocked;

                if (isUnlocked)
                {
                    button.onClick.AddListener(() => OnLevelClicked(levelId));
                }
            }

            // 设置星级显示
            int stars = GameManager.Instance.GetLevelStars(levelId);
            UpdateStarDisplay(levelBtn, stars);

            levelButtons.Add(levelBtn);
        }
    }

    /// <summary>
    /// 更新星级显示
    /// </summary>
    private void UpdateStarDisplay(GameObject levelBtn, int stars)
    {
        // 假设子物体命名为 Star1, Star2, Star3
        for (int i = 1; i <= 3; i++)
        {
            var star = levelBtn.transform.Find($"Star{i}");
            if (star != null)
            {
                star.gameObject.SetActive(i <= stars);
            }
        }
    }

    /// <summary>
    /// 关卡按钮点击
    /// </summary>
    private void OnLevelClicked(int levelId)
    {
        Debug.Log($"Selected level: {levelId}");
        GameManager.Instance.StartLevel(levelId);
        SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 返回按钮点击
    /// </summary>
    private void OnBackClicked()
    {
        SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// 切换章节
    /// </summary>
    public void SwitchChapter(int chapter)
    {
        if (chapter < 1 || chapter > totalChapters) return;

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
}
