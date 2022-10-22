using System;  
using NUnit.Framework;
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Threading;
using System.Collections.Generic;
using System.Text.Json;
using Newtonsoft.Json;


namespace Chat {
    
    
    [TestFixture]
    public class TestServidor  
    {  
        #pragma warning disable CS8600
        #pragma warning disable CS8602
        private static IPHostEntry host = Dns.GetHostEntry("localhost");  
        private static IPAddress ipAddress = host.AddressList[0];  
        private Socket cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 

        [Test]
        //prueba que si un usuario no se ha identificado no puede hacer nada hasta que se identifique
        public void TestIdentify() {
            Servidor servidor = new Servidor("localhost", 1236);
            Thread hilo = new Thread(servidor.Inicia);

            hilo.Start();
            Thread.Sleep(2000);

            Socket cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1236));

            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "PUBLIC_MESSAGE");
            json.Add("message", "hola mundo");
            String mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            byte[] bytes = new byte[1024];
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

        }

        [Test]
        //prueba que si se manda un mensaje incompleto se responde con un error
        public void TestMensajeIncompleto() {
            Servidor servidor = new Servidor("localhost", 1235);
            Thread hilo = new Thread(servidor.Inicia);

            hilo.Start();
            Thread.Sleep(2000);

            Socket cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));

            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "IDENTIFY");
            String mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            byte[] bytes = new byte[1024];
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));
            json.Clear();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json.Clear();
            json.Add("type", "STATUS");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");
            json.Add("status", "hola mundo");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));
            json.Clear();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json.Clear();
            json.Add("type", "MESSAGE");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));
            json.Clear();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json.Clear();
            json.Add("type", "PUBLIC_MESSAGE");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));
            json.Clear();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json.Clear();
            json.Add("type", "NEW_ROOM");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));
            json.Clear();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json.Clear();
            json.Add("type", "INVITE");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));
            json.Clear();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json.Clear();
            json.Add("type", "JOIN_ROOM");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));
            json.Clear();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json.Clear();
            json.Add("type", "ROOM_USERS");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));
            json.Clear();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json.Clear();
            json.Add("type", "ROOM_MESSAGE");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));
            json.Clear();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json.Clear();
            json.Add("type", "LEAVE_ROOM");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            

        }

        [Test]
        //prueba que cualquier mensaje no reconocido se responda con un error
        public void TestMensajeNoReconocido() {
            Servidor servidor = new Servidor("localhost", 1237);
            Thread hilo = new Thread(servidor.Inicia);

            hilo.Start();
            Thread.Sleep(2000);

            Socket cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1237));

            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            String mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            byte[] bytes = new byte[1024];
            cliente.Receive(bytes, 1024, 0);
            json.Clear();
            json.Add("type", "hola mundo");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));
            json.Clear();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            mensaje = "hola mundo";
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

            cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(new IPEndPoint(ipAddress, 1235));
            json.Clear();
            json.Add("type", "IDENTIFY");
            json.Add("username", "usuario");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json.Clear();
            json.Add("type", "INVITE");
            json.Add("roomname", "cuarto");
            json.Add("usernames", "usuarios");
            mensaje = JsonConvert.SerializeObject(json);
            cliente.Send(Parser.CadenaABytes(mensaje), mensaje.Length, 0);
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");
        }
        
    }
}