using UnityEngine;
using UnityEngine.SceneManagement;
public class scene_script : MonoBehaviour
{
    public void LoadNextScene(string sceneName)
    {

        SceneManager.LoadScene(sceneName);
    }

}
