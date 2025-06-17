using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void RunSinglePlayer()
    {
        SceneManager.LoadScene(1);
    }

    public void RunMultiPlayer()
    {

    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
