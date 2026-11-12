using UnityEngine;
using TMPro;

public class NPCInteract : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    [TextArea] public string line = "Do you know what's going on?";
    public float maxDistance = 4f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(
                new Vector3(Screen.width / 2f, Screen.height / 2f));

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance)
                && hit.collider.gameObject == gameObject)
            {
                dialogueText.text = line;
                dialoguePanel.SetActive(true);
            }
            else
            {
                dialoguePanel.SetActive(false);
            }
        }
    }
}