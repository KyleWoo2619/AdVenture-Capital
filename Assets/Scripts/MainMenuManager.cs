using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    //public delegate void ClickDelegate(); //defines a delegate of type ClickDelegate
    //public static event ClickDelegate OnClicked; //this is a delegate of type ClickDelegate

    //public static event Action Click; //always has a void return type, but can have parameters


    public void LoadConstructionJump()
    {
        SceneManager.LoadScene("TheBlockGame", LoadSceneMode.Single);
    }

    public void LoadCandyJump()
    {
        SceneManager.LoadScene("BonBonCrushAP", LoadSceneMode.Single);
    }

    public void ReturnToStart()
    {
        SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
    }
}
