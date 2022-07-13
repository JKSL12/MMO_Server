﻿using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();

    public void Send(ArraySegment<byte> sendBuff)
    {
        _session.Send(sendBuff);
    }

    void Start()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connecter = new Connector();

        connecter.Connect(endPoint, () => { return _session; }, 1);
    }

    void Update()
    {
        List<IPacket> list = PacketQueue.Instance.PopAll();
        foreach(IPacket packet in list)
        {
            if (packet != null)
            {
                PacketManager.Instance.HandlePacket(_session, packet);
            }
        }
        //IPacket packet = PacketQueue.Instance.Pop();
        //if(packet != null)
        //{
        //    PacketManager.Instance.HandlePacket(_session, packet);
        //}
    }    
}
