using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraControll : MonoBehaviour
{
    public Transform playerTranform;
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(playerTranform.position.x, playerTranform.position.y, transform.position.z);
    }
}
