using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  

namespace Chat {
    public class Servidor  
    {  
        
  	public class main
	    {          
        public static void Main(string[] args)
        {   

            Inicia();      
        }
    }
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