using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;


public class AccelerometerSetup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UduinoManager.Instance.OnDataReceived += DataReceived;

    }

    public void DataReceived(string data, UduinoDevice board){
        Debug.Log(data);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
