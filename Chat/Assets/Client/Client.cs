using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public GameObject LoginPanel;
    public GameObject ChatDeFala;
    public GameObject MensagemPrefab;

    string nome = "";

    private bool socketReady = false;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;
    public void ConnectToServer()
    {

        if (socketReady)
        {
            return;
        }
        //Se estiver conectado aciona com valores default
        string host = "127.0.0.1";
        int port = 6331;

        //Reajusta valores se algo for digitado
        string h;
        int p;
        string n;

        h = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (h != "")
        {
            host = h;
        }
        int.TryParse(GameObject.Find("PortInput").GetComponent<InputField>().text, out p);
        if (p != 0)
        {
            port = p;
        }
        n = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (n != "")
        {
            nome = n;
        }
        // Criar o socket

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            LoginPanel.SetActive(false);
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    void Start()
    {

    }


    void Update()
    {
        //Estou recebendo mensagem
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(data);
                }
            }
        }
    }
    void OnIncomingData(string data)
    {
        if (data == "@NOMEE")
        {
            Send("#NOMEE|" + nome);
            return;
        }
        GameObject ms = Instantiate(MensagemPrefab, ChatDeFala.transform) as GameObject;
        ms.GetComponentInChildren<Text>().text = data;
    }

    private void Send(string data)
    {
        if (!socketReady)
        {
            return;
        }

        writer.WriteLine(data);
        writer.Flush();
    }

    public void OnSendButton()
    {
        string message = GameObject.Find("SendInput").GetComponent<InputField>().text;
        Send(message);
    }
}
