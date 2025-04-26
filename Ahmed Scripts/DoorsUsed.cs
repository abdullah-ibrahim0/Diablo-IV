using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorsUsed : MonoBehaviour
{
    private PlayerBehavior player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnterDetect"))
        {
            player.doorsUsed++;
            Debug.Log("DOOR NO.: " + player.doorsUsed);

        }
    }
}
