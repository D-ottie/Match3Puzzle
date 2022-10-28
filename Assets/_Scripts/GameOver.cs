using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    public GameObject panelParent;
    public GameObject scoreParent;

    public TMPro.TextMeshProUGUI loseText;
    public TMPro.TextMeshProUGUI winText;
    public TMPro.TextMeshProUGUI scoreText;
    public GameObject[] panelStars;
    // Start is called before the first frame update
    void Start()
    {
        panelParent.SetActive(false);
        for (int i = 0; i < panelStars.Length; i++)
        {
            panelStars[i].SetActive(false);
        }
    }
    public void ShowLose()
    {
        panelParent.SetActive(true);
        scoreParent.SetActive(false);
        winText.enabled = false;
        loseText.enabled = true;
        AnimatePanel();
    }
    public void ShowWin(int score, int starCount)
    {
        panelParent.SetActive(true);
        loseText.enabled = false; 
        winText.enabled = true;

        scoreText.SetText(score.ToString());
        scoreText.enabled = true;
        AnimatePanel();

        StartCoroutine(ShowWinCoroutine(starCount)); 
    }

    public void AnimatePanel()
    {
        Animator _animator = GetComponent<Animator>();
        if (_animator)
            _animator.Play("GameOverShow");
    }

    private IEnumerator ShowWinCoroutine(int starCount)
    {
        yield return new WaitForSeconds(0.5f); 
        if (starCount < panelStars.Length)
        {
            for (int i = 0; i <= starCount; i++)
            {
                panelStars[i].SetActive(true); 
                if (i > 0)
                {
                    panelStars[i - 1].SetActive(false); 
                }
                yield return new WaitForSeconds(0.5f); 
            }
        }
        scoreText.enabled = true; 
    }

    public void OnReplayClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.
            SceneManagement.
            SceneManager.GetActiveScene().buildIndex); 
    }
    public void OnDoneClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelect"); 
    }

}
