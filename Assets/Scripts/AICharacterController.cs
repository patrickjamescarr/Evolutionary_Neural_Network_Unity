using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICharacterController : MonoBehaviour
{
    private AIFSM stateMachine;

    // Start is called before the first frame update
    void Start()
    {
        stateMachine = new AIFSM();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
