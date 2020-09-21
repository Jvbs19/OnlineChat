using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;

public class Server : MonoBehaviour
{
    private List<ServerClient> clients; // Clientes conectados
    private List<ServerClient> disconnectList; //Clientes desconectados

    public int port = 6321;
    private TcpListener server;
    private bool serverStarted; // Saber se o server ja iniciou

    // Start is called before the first frame update
    void Start()
    {
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port); // Recebe conexão
            server.Start(); // Começar conexão

            StartListening();
            serverStarted = true;
            Debug.Log("Server Inicializado na porta " + port);
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;

        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        StartListening();

        Broadcast("@NOMEE", new List<ServerClient>() { clients[clients.Count - 1] });
        // Enviar uma mensagem a todos conectados, dizendo que alguem foi conectado;
        // Broadcast(clients[clients.Count - 1].clientName + "Se conectou", clients);
    }

    private void OnIncomingData(ServerClient c, string data)
    {
        if (data.Contains("#NOMEE"))
        {
            c.clientName = data.Split('|')[1];
            Broadcast(c.clientName + " has connected", clients);
            return;
        }
        Broadcast(c.clientName + " : " + data, clients);
    }
    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient c in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("write error: " + e.Message + "To  client" + c.clientName);
            }
        }
    }

    void Update()
    {
        if (!serverStarted) // Se o server nao tiver iniciado ele não vai executar o codigo
        {
            return;
        }
        foreach (ServerClient c in clients)// Para cada cliente conectado meu cliente "c"
        {
            // Perguntar se o cliente for conectado?
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            // Se ele estiver conectado. Procure mensagens do cliente
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(c, data);
                    }
                }
            }
        }
        for (int i = 0; i < disconnectList.Count -1; i++)
        {
            Broadcast(disconnectList[i].clientName +  " has disconnected", clients);
            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }
    }
}

public class ServerClient// Usado para definir quem esta conectando. visivel apenas no servidor
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }
   
}
