using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 新手引导步骤数据
/// </summary>
[Serializable]
public class TutorialStep
{
    public string stepId;
    public string title;
    public string description;
    public Sprite tutorialImage;        // 引导图片（可选）
    public Vector2 highlightPosition;   // 高亮区域位置
    public Vector2 highlightSize;      // 高亮区域大小
    public RectTransform anchorTarget;  // 箭头指向的目标
    public float duration = 0f;         // 持续时间（0表示等待点击）
    public bool requireAction = false;  // 是否需要执行特定操作
    public TutorialAction requiredAction; // 需要执行的操作类型
    public string actionParameter;      // 操作参数
}

/// <summary>
/// 引导操作类型
/// </summary>
public enum TutorialAction
{
    None,
    Swipe,             // 滑动
    Tap,               // 点击
    LongPress,         // 长按
    SwipeDirection     // 特定方向滑动
}

/// <summary>
/// 新手引导管理器
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("引导配置")]
    [SerializeField] private TutorialStep[] tutorialSteps;
    [SerializeField] private bool skipTutorial = false;  // 跳过引导（调试用）

    [Header("UI组件")]
    [SerializeField] private GameObject tutorialPanel;       // 引导面板
    [SerializeField] private Image tutorialImage;             // 引导图片
    [SerializeField] private Text titleText;                 // 标题
    [SerializeField] private Text descriptionText;           // 描述
    [SerializeField] private Image handIndicator;            // 手指指示器
    [SerializeField] private Image arrowIndicator;            // 箭头指示器
    [SerializeField] private Button skipButton;               // 跳过按钮
    [SerializeField] private Button nextButton;               // 下一步按钮
    [SerializeField] private GameObject highlightArea;        // 高亮区域

    private int currentStepIndex = -1;
    private bool isTutorialActive = false;
    private List<string> completedSteps = new List<string>();

    private const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";

    public event Action<string> OnTutorialStepCompleted;
    public event Action OnTutorialCompleted;
    public event Action<string> OnTutorialStarted;

    private void Start()
    {
        // 检查是否已完成引导
        if (IsTutorialCompleted())
        {
            Debug.Log("Tutorial already completed.");
            return;
        }

        // 初始化UI
        InitUI();

        // 开始引导
        StartTutorial();
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitUI()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTutorial);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextStep);
        }
    }

    /// <summary>
    /// 开始引导
    /// </summary>
    public void StartTutorial()
    {
        if (skipTutorial)
        {
            CompleteTutorial();
            return;
        }

        isTutorialActive = true;
        currentStepIndex = -1;

        // 隐藏游戏UI
        // TODO: 通知游戏UI隐藏

        // 开始第一步
        NextStep();

        OnTutorialStarted?.Invoke("Tutorial");
    }

    /// <summary>
    /// 执行下一步
    /// </summary>
    public void NextStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= tutorialSteps.Length)
        {
            CompleteTutorial();
            return;
        }

        var step = tutorialSteps[currentStepIndex];

        // 检查步骤是否已完成
        if (completedSteps.Contains(step.stepId))
        {
            NextStep();
            return;
        }

        // 显示引导
        ShowStep(step);
    }

    /// <summary>
    /// 显示引导步骤
    /// </summary>
    private void ShowStep(TutorialStep step)
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }

        // 更新文本
        if (titleText != null)
        {
            titleText.text = step.title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = step.description;
        }

        // 更新图片
        if (tutorialImage != null && step.tutorialImage != null)
        {
            tutorialImage.sprite = step.tutorialImage;
            tutorialImage.gameObject.SetActive(true);
        }
        else if (tutorialImage != null)
        {
            tutorialImage.gameObject.SetActive(false);
        }

        // 更新高亮区域
        UpdateHighlightArea(step);

        // 更新指示器
        UpdateIndicators(step);

        // 更新下一步按钮
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(step.duration == 0 && !step.requireAction);
        }

        // 如果有持续时间，自动等待
        if (step.duration > 0)
        {
            StartCoroutine(AutoAdvanceCoroutine(step.duration));
        }
    }

    /// <summary>
    /// 更新高亮区域
    /// </summary>
    private void UpdateHighlightArea(TutorialStep step)
    {
        if (highlightArea != null)
        {
            if (step.anchorTarget != null)
            {
                // 跟随目标
                highlightArea.SetActive(true);
                // TODO: 设置高亮区域位置和大小
            }
            else if (step.highlightSize.magnitude > 0)
            {
                highlightArea.SetActive(true);
                // TODO: 设置指定位置的高亮
            }
            else
            {
                highlightArea.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 更新指示器
    /// </summary>
    private void UpdateIndicators(TutorialStep step)
    {
        // 手指指示器
        if (handIndicator != null)
        {
            handIndicator.gameObject.SetActive(step.requireAction && step.requiredAction == TutorialAction.Swipe);
        }

        // 箭头指示器
        if (arrowIndicator != null)
        {
            arrowIndicator.gameObject.SetActive(step.anchorTarget != null);
        }
    }

    /// <summary>
    /// 自动前进协程
    /// </summary>
    private System.Collections.IEnumerator AutoAdvanceCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (isTutorialActive && currentStepIndex >= 0 && currentStepIndex < tutorialSteps.Length)
        {
            CompleteCurrentStep();
        }
    }

    /// <summary>
    /// 完成当前步骤
    /// </summary>
    public void CompleteCurrentStep()
    {
        if (currentStepIndex < 0 || currentStepIndex >= tutorialSteps.Length)
            return;

        var step = tutorialSteps[currentStepIndex];
        completedSteps.Add(step.stepId);

        OnTutorialStepCompleted?.Invoke(step.stepId);

        // 记录分析数据
        AnalyticsManager.Instance?.LogTutorialStep(step.stepId, true);

        // 隐藏当前面板，显示下一步
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // 延迟显示下一步
        Invoke(nameof(NextStep), 0.3f);
    }

    /// <summary>
    /// 完成引导
    /// </summary>
    public void CompleteTutorial()
    {
        isTutorialActive = false;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // 标记引导完成
        PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 1);
        PlayerPrefs.Save();

        // 显示游戏UI
        // TODO: 通知游戏UI显示

        OnTutorialCompleted?.Invoke();

        Debug.Log("Tutorial completed!");
    }

    /// <summary>
    /// 跳过引导
    /// </summary>
    public void SkipTutorial()
    {
        Debug.Log("Tutorial skipped.");
        CompleteTutorial();
    }

    /// <summary>
    /// 检查引导是否完成
    /// </summary>
    public bool IsTutorialCompleted()
    {
        return PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1;
    }

    /// <summary>
    /// 重置引导状态（用于测试）
    /// </summary>
    public void ResetTutorial()
    {
        PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 0);
        completedSteps.Clear();
        currentStepIndex = -1;
    }

    /// <summary>
    /// 处理引导操作
    /// </summary>
    public void HandleTutorialAction(TutorialAction action, string parameter = "")
    {
        if (!isTutorialActive || currentStepIndex < 0 || currentStepIndex >= tutorialSteps.Length)
            return;

        var step = tutorialSteps[currentStepIndex];

        if (!step.requireAction)
            return;

        // 检查动作是否匹配
        bool actionMatched = false;

        switch (step.requiredAction)
        {
            case TutorialAction.Tap:
                actionMatched = action == TutorialAction.Tap;
                break;

            case TutorialAction.Swipe:
            case TutorialAction.SwipeDirection:
                actionMatched = action == TutorialAction.Swipe;
                // TODO: 检查滑动方向
                break;

            case TutorialAction.LongPress:
                actionMatched = action == TutorialAction.LongPress;
                break;
        }

        if (actionMatched)
        {
            CompleteCurrentStep();
        }
    }
}
