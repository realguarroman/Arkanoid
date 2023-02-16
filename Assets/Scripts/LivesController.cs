using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class LivesController : NetworkBehaviour
{
    Text textcomp;

    [Networked]
    public int lives { get; set; }

    private void Awake(){
        textcomp = GetComponent<Text>();
    }

    public override void Spawned() {
        lives = 5;
    }

    public void ReduceLives() {
        if (lives > 0) {
            lives--;
            textcomp.text = $"<color=#feae34>Lives: {lives}</color>";
        }
    }
}


