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
        private Controlador.ControladorVista controlador;

        public static void Main()  
        {  
            
            Cliente cliente = new Cliente();
            cliente.Inicia();  
            cliente.controlador = new Controlador.ControladorVista(cliente);
            cliente.controlador.PideNombre();
        }  

        public Cliente() {
            controlador = new Controlador.ControladorVista(this);
        }
  
        public void Inicia()  
        {  
            byte[] bytes = new byte[2048];  
  
            try  
            {  

            
  
                try  
                {  
                    enchufe.Connect(remoteEP);  
  
                    controlador.Mensaje("Enchufe conectado a " +  
                    enchufe.RemoteEndPoint.ToString());  
  
                
  
                }  
                catch (ArgumentNullException ane)  
                {  
                    controlador.Mensaje("ArgumentNullException : " + ane.ToString());  
                }  
                catch (SocketException se)  
                {  
                    controlador.Mensaje("SocketException : " + se.ToString());  
                }  
                catch (Exception e)  
                {  
                    controlador.Mensaje("Unexpected exception : " + e.ToString());  
                }  
  
            }  
            catch (Exception e)  
            {  
                controlador.Mensaje(e.ToString());  
            }  
        }  

        
        public void Identifica(String nombre) {

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("type", "IDENTIFY");
            dic.Add("message", nombre);
            String mensaje = JsonConvert.SerializeObject(dic);
            try {
                enchufe.Send(CadenaABytes(mensaje));
            } catch(SocketException se) {
                controlador.Mensaje("Ocurrió un error al conectarse con el servidor");
                enchufe.Close();
                Environment.Exit(0);
            }
        }

        //convierte una cadena en un arreglo de bytes para mandarlo por el enchufe
            private byte[] CadenaABytes(String cadena) {
                return Encoding.UTF8.GetBytes(cadena);
            }

    } 
}