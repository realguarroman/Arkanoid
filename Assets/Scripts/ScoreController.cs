using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : NetworkBehaviour
{
    Text textcomp;
    int score = 0;

    private void Awake() {
        textcomp = GetComponent<Text>();
    }
    public void IncreaseScore() {
        score+=5;
        textcomp.text = $"<color=#feae34>Score: {score}</color>";
    }
}


