using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public event Action OnStartingBattle;
    public void PlayGame()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        OnStartingBattle();
    }
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
