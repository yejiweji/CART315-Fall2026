using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BirdSkin
{
    public string name = "Skin";
    [Tooltip("XP charged for each attempt, whether or not it unlocks.")]
    public int cost = 50;
    [Tooltip("Chance (0-1) that a purchase actually unlocks the skin. 0.5 = 50/50.")]
    [Range(0f, 1f)] public float chance = 0.5f;
    [Tooltip("Color applied to the bird. Works with no art at all.")]
    public Color tint = Color.white;
    [Tooltip("Optional: your own wing-flap frames. Leave empty to reuse the normal bird with the tint.")]
    public Sprite[] sprites;
}

// Spend XP to try to unlock bird skins. Rarer skins cost more and unlock less often.
public class SkinShop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private LossRewards rewards;
    [Tooltip("Small panel with a Horizontal (or Vertical) Layout Group. Do NOT make it full-screen.")]
    [SerializeField] private GameObject shopPanel;
    [Tooltip("One Button (with a Text child) inside the panel. It gets copied for each skin.")]
    [SerializeField] private Button buttonTemplate;
    [Tooltip("Shows 'UNLOCKED!' / 'No luck' messages. Put it outside the layout panel.")]
    [SerializeField] private Text resultText;

    [Header("In-run display (look only, not clickable)")]
    [Tooltip("Small strip with a Horizontal Layout Group, shown only while flying.")]
    [SerializeField] private GameObject hudPanel;
    [Tooltip("One Image inside the HUD panel. It gets copied for each skin.")]
    [SerializeField] private Image hudIconTemplate;
    [Tooltip("Size in pixels of each in-run icon. Set here so the layout group can't shrink it.")]
    [SerializeField] private float hudIconSize = 400f;
    [Tooltip("The 'NEXT: EMBER - 50 XP (you have 30)' line.")]
    [SerializeField] private Text hudText;

    [Header("Skins (better skin = pricier and rarer)")]
    [SerializeField] private List<BirdSkin> skins = new List<BirdSkin>
    {
        new BirdSkin { name = "Ember",    cost = 50,  chance = 0.50f, tint = new Color(1.00f, 0.55f, 0.20f) },
        new BirdSkin { name = "Rose",     cost = 75,  chance = 0.40f, tint = new Color(1.00f, 0.50f, 0.70f) },
        new BirdSkin { name = "Mint",     cost = 100, chance = 0.30f, tint = new Color(0.40f, 1.00f, 0.70f) },
        new BirdSkin { name = "Midnight", cost = 150, chance = 0.20f, tint = new Color(0.40f, 0.40f, 0.95f) },
        new BirdSkin { name = "Golden",   cost = 250, chance = 0.10f, tint = new Color(1.00f, 0.84f, 0.00f) },
    };

    private static SkinShop current;

    private bool[] owned;
    private Button[] buttons;
    private Text[] labels;
    private int equipped = 0;      // 0 = classic bird, 1..N = skins[index - 1]
    private bool hasDied;
    private Image[] hudIcons;
    private Sprite classicIcon;
    private bool hudVisible;

    private void Awake()
    {
        current = this;
    }

    private void OnDestroy()
    {
        if (current == this) current = null;
    }

    private void Start()
    {
        owned = new bool[skins.Count];

        // Button 0 = classic bird (always owned), buttons 1..N = the skins
        int count = skins.Count + 1;
        buttons = new Button[count];
        labels = new Text[count];

        buttonTemplate.gameObject.SetActive(false);

        for (int i = 0; i < count; i++) {
            Button b = Instantiate(buttonTemplate, buttonTemplate.transform.parent);
            b.gameObject.SetActive(true);

            int index = i;
            b.onClick.AddListener(() => OnPressed(index));

            buttons[i] = b;
            labels[i] = b.GetComponentInChildren<Text>();
        }

        // In-run display: one small icon per skin
        classicIcon = (player.sprites != null && player.sprites.Length > 0) ? player.sprites[0] : null;

        if (hudIconTemplate != null) {
            hudIconTemplate.gameObject.SetActive(false);
            hudIcons = new Image[skins.Count];

            for (int i = 0; i < skins.Count; i++) {
                Image img = Instantiate(hudIconTemplate, hudIconTemplate.transform.parent);
                img.gameObject.SetActive(true);
                img.raycastTarget = false; // never eats a tap

                // Force the size here so the panel's layout group can't shrink the icon
                img.rectTransform.sizeDelta = new Vector2(hudIconSize, hudIconSize);
                img.preserveAspect = true;

                LayoutElement le = img.GetComponent<LayoutElement>();
                if (le == null) le = img.gameObject.AddComponent<LayoutElement>();
                le.minWidth = hudIconSize;
                le.minHeight = hudIconSize;
                le.preferredWidth = hudIconSize;
                le.preferredHeight = hudIconSize;
                le.flexibleWidth = 0f;
                le.flexibleHeight = 0f;

                BirdSkin skin = skins[i];
                Sprite icon = (skin.sprites != null && skin.sprites.Length > 0) ? skin.sprites[0] : classicIcon;
                if (icon != null) img.sprite = icon;

                hudIcons[i] = img;
            }
        }

        if (hudText != null) hudText.raycastTarget = false;

        Equip(0);
        Say("");
        if (shopPanel != null) shopPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (hudText != null) hudText.gameObject.SetActive(false);
    }

    private void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || shopPanel == null) return;

        if (gm.State == GameState.Dying) hasDied = true;

        // In-run display: visible only while flying
        bool hudShow = gm.State == GameState.Playing;

        if (hudShow != hudVisible) {
            hudVisible = hudShow;
            if (hudPanel != null) hudPanel.SetActive(hudShow);
            if (hudText != null) hudText.gameObject.SetActive(hudShow);
            if (hudShow) RefreshHud();
        }

        // Shop is visible between runs, hidden while flying
        bool show = hasDied && (gm.State == GameState.Dying || gm.State == GameState.Ready);

        if (show != shopPanel.activeSelf) {
            shopPanel.SetActive(show);

            if (show) RefreshLabels();
            else Say("");
        }
    }

    // GameManager asks this so tapping a shop button doesn't also restart the run
    public static bool BlocksTap(Vector2 screenPosition)
    {
        if (current == null || current.shopPanel == null || !current.shopPanel.activeInHierarchy) return false;

        RectTransform rect = current.shopPanel.GetComponent<RectTransform>();
        if (rect == null) return false;

        Canvas canvas = current.shopPanel.GetComponentInParent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

        return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, cam);
    }

    private void OnPressed(int index)
    {
        // Classic bird: always available
        if (index == 0) {
            Equip(0);
            Say("");
            return;
        }

        int skinIndex = index - 1;
        BirdSkin skin = skins[skinIndex];

        // Already owned: just equip it
        if (owned[skinIndex]) {
            Equip(index);
            Say("Equipped " + skin.name);
            return;
        }

        // Pay first; the XP is gone whether or not the roll succeeds
        if (!rewards.TrySpend(skin.cost)) {
            Say("Not enough XP!");
            return;
        }

        bool won = Random.value < skin.chance;

        if (won) {
            owned[skinIndex] = true;
            Equip(index);
            Say("UNLOCKED " + skin.name.ToUpper() + "!");
        } else {
            Say("So close! No luck this time.");
            RefreshLabels();
        }

        Debug.Log($"[SkinShop] tried {skin.name} cost={skin.cost} chance={skin.chance:P0} won={won} balanceLeft={rewards.Balance}");
    }

    private void Equip(int index)
    {
        equipped = index;

        if (index == 0) {
            player.ResetSkin();
        } else {
            BirdSkin skin = skins[index - 1];
            player.ApplySkin(skin.sprites, skin.tint);
        }

        RefreshLabels();
    }

    private void RefreshLabels()
    {
        if (labels == null) return;

        RefreshHud();

        for (int i = 0; i < labels.Length; i++) {
            if (labels[i] == null) continue;

            string state;
            string name;

            if (i == 0) {
                name = "CLASSIC";
                state = equipped == 0 ? "EQUIPPED" : "EQUIP";
            } else {
                BirdSkin skin = skins[i - 1];
                name = skin.name.ToUpper();

                if (owned[i - 1]) {
                    state = equipped == i ? "EQUIPPED" : "EQUIP";
                } else {
                    state = skin.cost + " XP | " + Mathf.RoundToInt(skin.chance * 100f) + "%";
                }
            }

            labels[i].text = name + "\n" + state;
        }
    }

    private void RefreshHud()
    {
        if (owned == null) return;

        // Icons: locked = dark silhouette, owned = full color, equipped = slightly bigger
        if (hudIcons != null) {
            for (int i = 0; i < hudIcons.Length; i++) {
                hudIcons[i].color = owned[i] ? skins[i].tint : new Color(0.1f, 0.1f, 0.1f, 0.65f);
                hudIcons[i].rectTransform.localScale = (equipped == i + 1) ? Vector3.one * 1.25f : Vector3.one;
            }
        }

        // Text: always point at the next locked skin
        if (hudText != null) {
            int next = -1;

            for (int i = 0; i < skins.Count; i++) {
                if (!owned[i]) { next = i; break; }
            }

            if (next < 0) {
                hudText.text = "ALL SKINS UNLOCKED";
            } else {
                BirdSkin target = skins[next];
                int have = rewards != null ? rewards.Balance : 0;

                hudText.text = have >= target.cost
                    ? target.name.ToUpper() + " UNLOCKABLE NOW - " + target.cost + " XP (you have " + have + ")"
                    : "NEXT: " + target.name.ToUpper() + " - " + target.cost + " XP (you have " + have + ")";
            }
        }
    }

    private void Say(string text)
    {
        if (resultText != null) resultText.text = text;
    }

}