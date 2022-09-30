using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Text.Json;
using Newtonsoft.Json;

  
namespace Cliente {
    public class Cliente  
    {  
        private static IPHostEntry host = Dns.GetHostEntry("localhost");  
        private static IPAddress ipAddress = host.AddressList[0];  
        private IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);  
  
        private static Socket enchufe = new Socket(ipAddress.AddressFamily,  
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

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("type", "IDENTIFY");
            dic.Add("message", nombre);
            String mensaje = JsonConvert.SerializeObject(dic);
            Console.WriteLine(mensaje);
            try {
                enchufe.Send(CadenaABytes(mensaje));
            } catch(SocketException se) {
                Console.WriteLine("Ocurrió un error al conectarse con el servidor");
                enchufe.Close();
                Environment.Exit(0);
            }
        }

        //convierte una cadena en un arreglo de bytes para mandarlo por el enchufe
            private byte[] CadenaABytes(String cadena) {
                return Encoding.ASCII.GetBytes(cadena);
            }

    } 
}