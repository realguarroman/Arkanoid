using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class LivesController : NetworkBehaviour
{
    Text textcomp;
    int lives = 5;

    private void Awake(){
        textcomp = GetComponent<Text>();
    }
    public void ReduceLives() {
        if (lives > 0) {
            lives--;
            textcomp.text = $"<color=#feae34>Lives: {lives}</color>";
        }
    }
}


