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

        private static IPHostEntry host = Dns.GetHostEntry("localhost");  
        private static IPAddress ipAddress = host.AddressList[0];  
        private static IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1234);    
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, 1234);  
        private Socket cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 

        [Test]
        //prueba que si un usuario no se ha identificado no puede hacer nada hasta que se identifique
        public void TestIdentify() {
            Servidor servidor = new Servidor("localhost", 1234);
            Thread hilo = new Thread(servidor.Inicia);

            hilo.Start();
            Thread.Sleep(2000);

            Socket cliente = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
            cliente.Connect(remoteEP);

            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "PUBLIC_MESSAGE");
            json.Add("message", "hola mundo");
            String mensaje = JsonConvert.SerializeObject(json);

            
            cliente.Send(Parser.CadenaABytes(mensaje), 1024, 0);
            byte[] bytes = new byte[1024];
            cliente.Receive(bytes, 1024, 0);
            json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Parser.BytesACadena(bytes));
            Assert.IsTrue(json["type"] == "ERROR");

        }

        
        
    }
}