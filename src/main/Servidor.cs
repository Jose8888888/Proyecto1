using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  

namespace Chat {
    public class Servidor  
    {  
        
  
        public static void Inicia()  
        {  
            IPHostEntry host = Dns.GetHostEntry("localhost");  
            IPAddress ipAddress = host.AddressList[0];  
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);    
      
            try {   
  
                Socket servidor = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  
                servidor.Bind(localEndPoint);  
                servidor.Listen(10);  
  
                Console.WriteLine("Esperando conexión...");  
                Socket cliente = servidor.Accept();  
  
                string data = null;  
                byte[] bytes = null;  
  

  
                Console.WriteLine("Cliente recibido");  
  

                cliente.Close();  
            }  
            catch (Exception e)  
            {  
                Console.WriteLine(e.ToString());  
            }  
  
        }          
    }
}