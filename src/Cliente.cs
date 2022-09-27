using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
  

public class Cliente  
{  
    public static int Main(String[] args)  
    {  
        Inicia();  
        return 0;  
    }  
  
    public static void Inicia()  
    {  
        byte[] bytes = new byte[1024];  
  
        try  
        {  

            IPHostEntry host = Dns.GetHostEntry("localhost");  
            IPAddress ipAddress = host.AddressList[0];  
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);  
  
            Socket enchufe = new Socket(ipAddress.AddressFamily,  
                SocketType.Stream, ProtocolType.Tcp);  
  
            try  
            {  
                enchufe.Connect(remoteEP);  
  
                Console.WriteLine("Enchufe conectado a {0}",  
                    enchufe.RemoteEndPoint.ToString());  
  
                byte[] mensaje = Encoding.ASCII.GetBytes("Hola ");  
                enchufe.Send(mensaje);
  
                int bytesRec = enchufe.Receive(bytes);  
                Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRec));  
  
                enchufe.Shutdown(SocketShutdown.Both);  
                enchufe.Close();  
  
            }  
            catch (ArgumentNullException ane)  
            {  
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());  
            }  
            catch (SocketException se)  
            {  
                Console.WriteLine("SocketException : {0}", se.ToString());  
            }  
            catch (Exception e)  
            {  
                Console.WriteLine("Unexpected exception : {0}", e.ToString());  
            }  
  
        }  
        catch (Exception e)  
        {  
            Console.WriteLine(e.ToString());  
        }  
    }  
} 