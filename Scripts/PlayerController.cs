using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
using System.IO;
using System.Threading;

public class PlayerController : MonoBehaviour
{
    //1. 서버
    private TcpClient client;
    private NetworkStream stream;

    public int handleValue; //128
    public int accelValue; //0 ~ 225
    public int brakeValue; //0
    public int prehandleValue;
    public int preaccelValue;
    public int prebrakeValue;

    public float maxSteeringAngle = 360000f; // Maximum steering angle in degrees (e.g., 45 degrees)
    public float maxForwardVelocity = 0.1f; // Maximum forward force when accelerating
    public float maxReverseVelocity = 0.1f; // Maximum force when braking

    //for moving
    public float playerSpeed = 10f;
    public Rigidbody playerRigidbody;
    public float power = 30f;

    // Variables for controlling rotation speed
    private Quaternion initialRotation;
    private float rotationSpeed = 5f;

    private void Start()
    {
        ConnectToServer("127.0.0.1", 27015);
        playerRigidbody = GetComponent<Rigidbody>();
        handleValue = 128;
        accelValue = 0;
        brakeValue = 0;
        initialRotation = playerRigidbody.rotation;

        // ReceiveCharArrayFromServer를 별도의 스레드에서 실행
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
                else if(idx == 2)
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

    public void FixedUpdate()
    {
        float handleInput = (handleValue - 128f) / 128f; // Normalize handle value to the range of -1 to 1
        float accelInput = accelValue / 255f; // Normalize accel value to the range of 0 to 1
        float brakeInput = brakeValue / 255f; // Normalize brake value to the range of 0 to 1

        // Calculate the steering angle based on the handle input
        float steeringAngle = handleInput * maxSteeringAngle;

        // Apply the steering angle to the target rotation
        Quaternion targetRotation = initialRotation * Quaternion.Euler(0f, steeringAngle, 0f);

        // Apply smooth rotation interpolation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        // Calculate the forward velocity based on the accel input
        float forwardVelocity = accelInput * maxForwardVelocity;

        // Calculate the reverse velocity based on the brake input
        float reverseVelocity = brakeInput * maxReverseVelocity;

        // Calculate the total velocity
        float totalVelocity = forwardVelocity - reverseVelocity;

        // Apply the velocity to the player's rigidbody
        Vector3 velocity = transform.forward * totalVelocity;
        GetComponent<Rigidbody>().velocity = velocity;
    }




    /*
        public void FixedUpdate()
        {
            float handleInput = (handleValue - 128f) / 128f; // Normalize handle value to the range of -1 to 1
            float accelInput = accelValue / 255f; // Normalize accel value to the range of 0 to 1
            float brakeInput = brakeValue / 255f; // Normalize brake value to the range of 0 to 1

            // Calculate the steering angle based on the handle input
            float steeringAngle = handleInput * maxSteeringAngle;

            // Apply the steering angle to the target rotation
            Quaternion targetRotation = initialRotation * Quaternion.Euler(0f, steeringAngle, 0f);

            *//*// Apply smooth rotation interpolation
            playerRigidbody.MoveRotation(Quaternion.Slerp(playerRigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

            //accel and brake; >> addForce
            float temp = accelInput - brakeInput;
            playerRigidbody.velocity = transform.position * (temp/10);*//*
            // Calculate the forward force based on the accel input
            float forwardForce = accelInput * maxForwardForce;

            // Calculate the reverse force based on the brake input
            float reverseForce = brakeInput * maxReverseForce;

            // Calculate the total force
            float totalForce = forwardForce - reverseForce;

            // Apply the force to the player's rigidbody
            Vector3 movementForce = transform.forward * totalForce * power;
            playerRigidbody.AddForce(movementForce);

        }*/
}