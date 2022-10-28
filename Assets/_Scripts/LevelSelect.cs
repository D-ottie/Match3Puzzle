using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    [System.Serializable]
    public struct ButtonPlayerPrefs
    {
        public GameObject _gameObject;
        public string playerPrefKey;
    }
    public ButtonPlayerPrefs[] buttons;
    public void OnButtonPress(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    private void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int score = PlayerPrefs.GetInt(buttons[i].playerPrefKey, 0);
            for (int starIdx = 0; starIdx <= 3; starIdx++)
            {
                Transform star = buttons[i]._gameObject.transform.Find("panel_Stars" + starIdx);
                if (starIdx <= score)
                {
                    star.gameObject.SetActive(true);
                }
                else star.gameObject.SetActive(false);
            }
        }
    }
}
