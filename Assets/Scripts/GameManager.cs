using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TileBoard board;
    public CanvasGroup gameOver;
    public CanvasGroup gameWon;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highscoreText;

    private int score;
    private int highscore;
    private bool keepGoing;
    private void Start() {
        NewGame();
    }

    public void NewGame(){
        keepGoing = false;
        score = 0;
        highscoreText.text = PlayerPrefs.GetInt("highscore",0).ToString();
        gameOver.alpha = 0f;
        gameOver.interactable = false;
        gameWon.alpha = 0f;
        gameWon.interactable = false;

        board.ClearBoard();
        board.CreateTile();
        board.CreateTile();
        board.enabled = true;
    }

    public void GameOver(){
        board.enabled = false;
        gameWon.blocksRaycasts = false;
        gameOver.interactable = true;
        StartCoroutine(Fade(gameOver,1f,1f));
    }

    public void GameWon(){
        board.enabled = false;
        gameOver.blocksRaycasts = false;
        gameWon.interactable = true;
        StartCoroutine(Fade(gameWon,1f,1f));
    }

    public void Continue(){
        gameWon.alpha = 0f;
        gameWon.interactable = false;
        board.enabled = true;
    }

    public void SetKeepGoing(){
        this.keepGoing = true;
        Continue();
    }
    public bool GetKeepGoing(){
        return keepGoing;
    }
    public void SetScore(int score){
        this.score += score;
        scoreText.text = this.score.ToString();

        SaveHighScore();
    }
    private void SaveHighScore(){
        int highScore = PlayerPrefs.GetInt("highscore",0);
        if(score > highScore){
            PlayerPrefs.SetInt("highscore", score);
        }
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay){
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;
        while(elapsed < duration){
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed/duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
