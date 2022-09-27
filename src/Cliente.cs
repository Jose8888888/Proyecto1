using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
  
namespace Cliente {
    public class Cliente  
    {  
        private static IPHostEntry host = Dns.GetHostEntry("localhost");  
        private static IPAddress ipAddress = host.AddressList[0];  
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);  
  
        Socket enchufe = new Socket(ipAddress.AddressFamily,  
                    SocketType.Stream, ProtocolType.Tcp);  

        public static void Main()  
        {  
            Cliente cliente = new Cliente();
            cliente.Inicia();  
            Vista.Vista vista = new Vista.Vista();
            vista.PideNombre();
        }  
  
        public void Inicia()  
        {  
            byte[] bytes = new byte[2048];  
  
            try  
            {  

            
  
                try  
                {  
                    enchufe.Connect(remoteEP);  
  
                    Console.WriteLine("Enchufe conectado a {0}",  
                    enchufe.RemoteEndPoint.ToString());  
  
                
  
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

        public void Identifica(String nombre) {
            foreach(byte b in CadenaABytes("IDENTIFY " + nombre)) {
                Console.WriteLine(b);
            }
            enchufe.Send(CadenaABytes("IDENTIFY " + nombre));
            foreach(byte b in CadenaABytes("IDENTIFY " + nombre)) {
                Console.WriteLine(b);
            }
        }

        //convierte una cadena en un arreglo de bytes para mandarlo por el enchufe
            private byte[] CadenaABytes(String cadena) {
                return Encoding.ASCII.GetBytes(cadena);
            }

    } 
}