using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : NetworkBehaviour
{
    Text textcomp;

    [Networked]
    public int score { get; set; }

    private void Awake() {
        textcomp = GetComponent<Text>();
    }

    public override void Spawned() {
        score = 5;
    }


    public void IncreaseScore() {
        score += 5;
        textcomp.text = $"<color=#feae34>Score: {score}</color>";
    }
}


