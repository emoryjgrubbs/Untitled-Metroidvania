using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractWithMap : MonoBehaviour
{
    // MAY NEED TO CHANGE THE POSITION IT TELEPORTS TO, I"M NOT SURE HOW UNITY WORKS WITH POSITION AND SCREEN MOVEMENT
    private Vector2 targetPosition;
    // Start is called before the first frame update
    void Start()
    {
        targetPosition.y = 5000f;
        transform.position = targetPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.H)){
            targetPosition.x = 0f;  //center of the screen x
            targetPosition.y = 0f;  //center of the screen y
            transform.position = targetPosition;    //teleports the map to the center of the screen
        }
        if(Input.GetKeyUp(KeyCode.M) || Input.GetKeyUp(KeyCode.H)){
            targetPosition.y = 5000f;   //y high above player
            transform.position = targetPosition;    //teleports the map off the screen
        }
    }
}
