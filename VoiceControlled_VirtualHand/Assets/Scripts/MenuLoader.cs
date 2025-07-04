using UnityEngine;
using UnityEngine.SceneManagement; //loadscene


public class MenuLoader : MonoBehaviour
{
    public void LoadLeftHand()
    {
        SceneManager.LoadScene("Left_VCVirtualHand");
    }

    public void LoadRightHand()
    {
        SceneManager.LoadScene("Right_VCVirtualHand");
    }

    public void LoadBothHands()
    {
        SceneManager.LoadScene("VCVirtualHands");
    }
}
