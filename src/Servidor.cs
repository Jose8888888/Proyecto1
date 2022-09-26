using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  

namespace Servidor {
    public class Servidor  
    {  
        
  	        
        public static void Main(string[] args)
        {   

            Inicia();      
        
    }
        public static void Inicia()  
        {  
            IPHostEntry host = Dns.GetHostEntry("localhost");  
            IPAddress ipAddress = host.AddressList[0];  
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);    
      
            try {   
  
                Socket escucha = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  
                escucha.Bind(localEndPoint);  
                escucha.Listen(10);  
  
                Console.WriteLine("Esperando conexión...");  
                Socket cliente = escucha.Accept();  


                string data = "";  
                byte[] bytes;
  
  
            
                bytes = new byte[1024];  
                int bytesRec = cliente.Receive(bytes);  
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);  

            
  
                Console.WriteLine("Cliente recibido");  
  
                byte[] mensaje = Encoding.ASCII.GetBytes(data + "mundo");  
                cliente.Send(mensaje);  
                cliente.Shutdown(SocketShutdown.Both);  
                cliente.Close();  
            }  
            catch (Exception e)  
            {  
                Console.WriteLine(e.ToString());  
            }  
  
        }          
    }
}
