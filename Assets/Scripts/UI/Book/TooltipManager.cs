using TMPro;
using UnityEngine;

public class TooltipManager : SingletonMonobehaviour<TooltipManager>
{
    [Header("Core")]
    [SerializeField] RectTransform tooltipPanel;
    [SerializeField] RectTransform tooltipParent; // BookUI or Canvas

    [Header("Text Fields")]
    [SerializeField] TMP_Text headerText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text contentText;
    [SerializeField] TMP_Text bonusText;

    public RectTransform TooltipRect => tooltipPanel;
    public RectTransform TooltipParent => tooltipParent;

    public TMP_Text HeaderText => headerText;
    public TMP_Text LevelText => levelText;
    public TMP_Text ContentText => contentText;
    public TMP_Text BonusText => bonusText;

    protected override void Awake()
    {
        base.Awake();

        if(tooltipPanel == null)
        {
            tooltipPanel = GetComponent<RectTransform>();
        }

        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("TooltipManager: TooltipPanel is missing!");
        }
    }

    public void Show()
    {
        if (tooltipPanel == null) return;
        tooltipPanel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (tooltipPanel == null) return;
        tooltipPanel.gameObject.SetActive(false);
    }
}
