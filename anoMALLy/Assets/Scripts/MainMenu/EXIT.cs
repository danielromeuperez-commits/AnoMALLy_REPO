using UnityEngine;

public class EXITBUTTON : MonoBehaviour
{
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Si estás en build, cierra el juego
        Application.Quit();
#endif
    }
}
