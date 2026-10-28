using UnityEngine;

namespace Cart315.FirstPerson
{
    // On-screen help text. The box now sizes itself to fit the text.
    public class PlaygroundInstructions : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private string titleText = "CART315 / LEAVE!";
        [SerializeField, TextArea(2, 4)] private string bodyText =
            "Move: WASD   Sprint: SHIFT     Jump: SPACE\nYou're not alone.";
        [SerializeField] private string clickPrompt = "Click to explore";

        [Header("Size")]
        [SerializeField, Range(10, 80)] private int titleSize = 34;
        [SerializeField, Range(10, 60)] private int bodySize = 22;

        [Header("Box")]
        [Tooltip("Untick to show the text with no black box.")]
        [SerializeField] private bool showBox = true;
        [Tooltip("Space between the text and the edge of the box, in pixels.")]
        [SerializeField, Range(0, 40)] private float padding = 10f;
        [Tooltip("Distance from the top-left corner of the screen, in pixels.")]
        [SerializeField, Range(0, 100)] private float margin = 16f;

        private GUIStyle _title, _body;

        private void OnGUI()
        {
            if (_title == null)
            {
                _title = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = false };
                _body = new GUIStyle(GUI.skin.label) { wordWrap = false };
            }

            _title.fontSize = titleSize;
            _body.fontSize = bodySize;

            Vector2 t = _title.CalcSize(new GUIContent(titleText));
            Vector2 b = _body.CalcSize(new GUIContent(bodyText));

            float width = Mathf.Max(t.x, b.x) + padding * 2f;
            float height = t.y + b.y + padding * 2f;

            if (showBox)
                GUI.Box(new Rect(margin, margin, width, height), GUIContent.none);

    
            GUI.Label(new Rect(margin + padding, margin + padding, t.x + 4f, t.y), titleText, _title);
            GUI.Label(new Rect(margin + padding, margin + padding + t.y, b.x + 4f, b.y), bodyText, _body);

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Vector2 c = _title.CalcSize(new GUIContent(clickPrompt));
                GUI.Label(new Rect(Screen.width / 2f - c.x / 2f, Screen.height / 2f, c.x + 4f, c.y), clickPrompt, _title);
            }
        }
    }
}