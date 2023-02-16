using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class LivesController : NetworkBehaviour
{
    Text textcomp;
    [SerializeField] private NetworkPrefabRef _panelPrefab;
    [SerializeField] private NetworkPrefabRef _gameoverPrefab;
    [SerializeField] private NetworkPrefabRef _gamePrefab;

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

            Runner.Spawn(_panelPrefab, new Vector3(0, 15, -3.0f));
            Runner.Spawn(_gamePrefab, new Vector3(0, 15, -3.5f));
        }
        else {
            Runner.Spawn(_panelPrefab, new Vector3(0, 15, -3.0f));
            Runner.Spawn(_gameoverPrefab, new Vector3(0, 15, -3.5f));
        }
    }
}


