using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameButtonScript : MonoBehaviour
{
    public void StartGame()
    {
        MultiplayerManager.Instance.StartGame();
    }
}
