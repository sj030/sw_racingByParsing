using System;
using System.Net.Sockets;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;



public class CarController : MonoBehaviour
{
    // 서버 s
    private TcpClient client;
    private NetworkStream stream;

    public int handleValue; //128
    public int accelValue; //0 ~ 225
    public int brakeValue; //0
    public int prehandleValue;
    public int preaccelValue;
    public int prebrakeValue;

    private void Start()
    {
        ConnectToServer("192.168.1.13", 27015);

        Thread receiveThread = new Thread(ReceiveDataThread);
        receiveThread.Start();
    }
    private void ReceiveDataThread()
    {
        while (true)
        {
            if (client.Connected && stream.DataAvailable)
            {
                ReceiveCharArrayFromServer();
            }
        }
    }
    private void ConnectToServer(string ipAddress, int port)
    {
        try
        {
            client = new TcpClient();
            client.Connect(ipAddress, port);
            stream = client.GetStream();
            Debug.Log("Connected to server");
            //ReceiveCharArrayFromServer();
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting to server: " + e.Message);
        }
    }
    private void ReceiveCharArrayFromServer() // in update() function 
    {
        Debug.Log("called ");
        try
        {
            int bufferSize = sizeof(char) * 14; // the buffer size

            byte[] buffer = new byte[bufferSize]; //whole data per frame
            char[] charArray = new char[bufferSize]; //dividing data 

            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string temp = "";
                for (int i = 0; i < bytesRead; i++)
                {
                    charArray[i] = (char)buffer[i];
                    temp += charArray[i];
                }
                //Debug.Log("Received data: " + temp);
                ProcessReceivedCharArray(charArray, bytesRead);
            }
        }
        catch (Exception e)
        {
            //Debug.Log("Error receiving char array from server: " + e.Message);
        }
    }

    public void ProcessReceivedCharArray(char[] charArray, int length)
    {
        // 이전값 저장 
        int idx = 0;
        string temp = "";
        for (int i = 0; i < length; i++)
        {
            if (charArray[i] == ',')
            {
                int value = Convert.ToInt32(temp);
                if (value > 260)
                {
                    Debug.Log("eee");
                    handleValue = prehandleValue;
                    accelValue = preaccelValue;
                    brakeValue = prebrakeValue;
                    break;
                }
                if (idx == 0)
                {
                    handleValue = value;
                    prehandleValue = handleValue;
                }
                else if (idx == 1)
                {
                    accelValue = value;
                    preaccelValue = accelValue;
                }
                else if (idx == 2)
                {
                    brakeValue = value;
                    prebrakeValue = brakeValue;
                }
                //init
                temp = "";
                idx++;
            }
            else
            {
                temp += charArray[i];
            }
        }

        Debug.Log("Received character: " + handleValue + ", " + accelValue + ", " + brakeValue);
    }

    private void OnDestroy()
    {
        if (client != null && client.Connected)
        {
            stream?.Close();
            client.Close();
        }
    }
    // 서버 e
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentbreakForce;
    private bool isBreaking;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteerAngle;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheeTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }


    private void GetInput()
    {
        if(accelValue >= 255 && brakeValue >= 255)
        {
            return;
        }
        horizontalInput = (handleValue - 128) / 128f ;
        if (accelValue < 10)
        {
            isBreaking = false;
            
        }
        else if (accelValue > 10)
        {
            isBreaking = true;
        }
        if (brakeValue < 10)
        {
            verticalInput = 0f;
        }
        else if(brakeValue > 10)
        {
            verticalInput = 0.5f;
        }
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentbreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();       
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot
;       wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}
