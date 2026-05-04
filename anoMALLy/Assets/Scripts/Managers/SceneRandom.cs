using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneRandom : MonoBehaviour
{
    public string[] escenas;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RandomSceneGenerator();
        }
    }

    void RandomSceneGenerator()
    {
        int indice = Random.Range(0, escenas.Length);
        SceneManager.LoadScene(escenas[indice]);
    }
}
