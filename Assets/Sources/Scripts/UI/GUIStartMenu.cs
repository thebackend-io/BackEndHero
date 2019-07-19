using UnityEngine;

public class GUIStartMenu : MonoBehaviour
{
    public GameObject guideText;

    public void GameStart() {
        if (!BackEndUIManager.instance.IsProcessing())
        {
            GameManager.instance.GameStart();
        }
    }
    
    private void OnEnable()
    {
        if (!guideText.activeSelf) {
            guideText.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (guideText.activeSelf) {
            guideText.SetActive(false);
        }
    }
}
