using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LossRewards : MonoBehaviour
{
    [Header("UI - shown after each death")]
    [SerializeField] private GameObject resultsPanel;   // parent of the four items below
    [SerializeField] private Text xpGainText;           // "+35 XP"
    [SerializeField] private Text levelText;            // "LEVEL 3"
    [SerializeField] private Slider xpBar;              // min 0, max 1, not interactable
    [SerializeField] private Text xpLabelText;          // "XP: 235" - counts up as the bar fills
    [SerializeField] private Text proofText;            // "You're in the top 12% today!"

    [Header("UI - always visible")]
    [SerializeField] private Text streakText;

    [Header("XP")]
    [Tooltip("Everyone gets this, even for a score of 0.")]
    [SerializeField] private int baseXp = 10;
    [SerializeField] private int xpPerPoint = 5;
    [Tooltip("Extra XP when the death was a 'near miss' to make a loss feel like progress.")]
    [SerializeField] private int nearMissBonusXp = 15;

    [Header("Bar (fills all session, never resets)")]
    [Tooltip("Total XP that fills the bar completely. Bigger = slower fill. The bar only resets when the session ends.")]
    [SerializeField] private int sessionXpGoal = 1500;
    [Tooltip("Level number goes up every this much XP. The bar does NOT reset on a level up.")]
    [SerializeField] private int xpPerLevel = 100;
    [SerializeField] private float barFillSpeed = 0.5f;

    [Header("Fake social proof (not real data)")]
    [Tooltip("Even a score of 0 says 'top this%'.")]
    [SerializeField] private int worstTopPercent = 45;
    [SerializeField] private int bestTopPercent = 3;
    [Tooltip("Each point scored lowers the shown percentage by this much.")]
    [SerializeField] private float percentPerPoint = 3f;

    [Header("Simulated streak (not real data)")]
    [Tooltip("Score this many in a session to 'save' the streak.")]
    [SerializeField] private int streakScoreTarget = 5;
    [SerializeField] private int minFakeStreak = 3;
    [SerializeField] private int maxFakeStreak = 9;

    private int totalXp = 0;

    private int dayStreak;
    private bool streakSecured;

    private Coroutine barRoutine;

    private void Awake()
    {
        if (xpBar != null) {
            xpBar.minValue = 0f;
            xpBar.maxValue = 1f;
        }
    }

    // ---- Called by GameManager ----

    public void OnSessionStart()
    {
        dayStreak = Random.Range(minFakeStreak, maxFakeStreak + 1);
        streakSecured = false;
        totalXp = 0;

        if (resultsPanel != null) resultsPanel.SetActive(false);
        UpdateLevelUI();
        ShowStreakInRun();

        Debug.Log($"[LossRewards] Fake starting streak = {dayStreak} days");
    }

    public void OnRunStart()
    {
        if (barRoutine != null) StopCoroutine(barRoutine);
        if (resultsPanel != null) resultsPanel.SetActive(false);
        ShowStreakInRun();
    }

    // Mid-run: hitting the target "saves" the streak
    public void OnScore(int score)
    {
        if (!streakSecured && score >= streakScoreTarget) {
            streakSecured = true;
            dayStreak++;
            SetStreak("STREAK SAVED!\n" + dayStreak + "-DAY STREAK");
        }
    }

    public void OnRunEnd(int score, bool nearMiss)
    {
        // 1. XP: always positive, even for a score of 0
        int gain = baseXp + score * xpPerPoint + (nearMiss ? nearMissBonusXp : 0);

        int startLevel = LevelFor(totalXp);
        float startFill = FillFor(totalXp);

        totalXp += gain;

        int newLevel = LevelFor(totalXp);
        float endFill = FillFor(totalXp);

        if (xpGainText != null) {
            xpGainText.text = newLevel > startLevel ? "+" + gain + " XP  LEVEL UP!" : "+" + gain + " XP";
        }

        // 2. Fake social proof: always flattering
        int topPercent = Mathf.RoundToInt(worstTopPercent - score * percentPerPoint + Random.Range(-4, 5));
        topPercent = Mathf.Clamp(topPercent, bestTopPercent, worstTopPercent);

        if (proofText != null) {
            proofText.text = "You're in the top " + topPercent + "% of players today!";
        }

        // 3. Streak: if not saved yet, warn that it's about to break
        if (!streakSecured) {
            SetStreak("STREAK ABOUT TO BREAK!\nScore " + streakScoreTarget + "+ to keep your " + dayStreak + "-day streak");
        }

        // Show it all, then animate the bar
        if (resultsPanel != null) resultsPanel.SetActive(true);
        if (barRoutine != null) StopCoroutine(barRoutine);
        barRoutine = StartCoroutine(AnimateBar(startLevel, startFill, newLevel, endFill));

        Debug.Log($"[LossRewards] score={score} xpGain={gain} totalXp={totalXp} level={newLevel} fakeTopPercent={topPercent} streakSecured={streakSecured}");
    }

    public void OnSessionEnd()
    {
        if (barRoutine != null) StopCoroutine(barRoutine);
        if (resultsPanel != null) resultsPanel.SetActive(false);
        SetStreak("");
    }

    // ---- Internals ----

    private int LevelFor(int xp)
    {
        return 1 + xp / Mathf.Max(1, xpPerLevel);
    }

    private float FillFor(int xp)
    {
        return Mathf.Clamp01(xp / (float)Mathf.Max(1, sessionXpGoal));
    }

    private IEnumerator AnimateBar(int startLevel, float startFill, int newLevel, float endFill)
    {
        if (levelText != null) levelText.text = "LEVEL " + startLevel;

        float f = startFill;
        SetBar(f);
        SetXpLabel(Mathf.RoundToInt(f * sessionXpGoal));

        // Bar only ever moves forward; it never resets mid-session
        while (f < endFill) {
            f = Mathf.MoveTowards(f, endFill, barFillSpeed * Time.deltaTime);
            SetBar(f);
            SetXpLabel(Mathf.RoundToInt(f * sessionXpGoal)); // number counts up with the bar
            yield return null;
        }

        SetBar(endFill);
        SetXpLabel(totalXp);
        if (levelText != null) levelText.text = "LEVEL " + newLevel;
    }

    private void ShowStreakInRun()
    {
        if (streakSecured) {
            SetStreak(dayStreak + "-DAY STREAK - SAVED");
        } else {
            SetStreak(dayStreak + "-DAY STREAK\nScore " + streakScoreTarget + "+ today to keep it!");
        }
    }

    private void UpdateLevelUI()
    {
        if (levelText != null) levelText.text = "LEVEL " + LevelFor(totalXp);
        SetBar(FillFor(totalXp));
        SetXpLabel(totalXp);
    }

    private void SetXpLabel(int value)
    {
        if (xpLabelText != null) xpLabelText.text = "XP: " + value;
    }

    private void SetBar(float value)
    {
        if (xpBar != null) xpBar.value = value;
    }

    private void SetStreak(string text)
    {
        if (streakText != null) streakText.text = text;
    }

}