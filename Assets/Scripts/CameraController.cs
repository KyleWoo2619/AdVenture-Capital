using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float cameraJump;
    //need a way for the camera to move with the player
    
    void Start()
    {
        cameraJump = 0.5f;
        //Debug.Log(cameraJump);
        StartCoroutine(CameraDelay(3));
       
    }
    public void MoveCamera()
    {
        transform.Translate(0, cameraJump, 0);
    }

    public IEnumerator CameraDelay(float time)
    {
        yield return new WaitForSeconds(time);
        for(;;)
        {
            MoveCamera();
            yield return new WaitForSeconds(time);
        }
    }
}
