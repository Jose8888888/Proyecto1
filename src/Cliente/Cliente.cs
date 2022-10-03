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
        private IPEndPoint remoteEP = new IPEndPoint(ipAddress, 1234);  
  
        private static Socket enchufe = new Socket(ipAddress.AddressFamily,  
                    SocketType.Stream, ProtocolType.Tcp);  
        private Controlador.ControladorVista controlador;
        private byte[] bytes = new byte[1024];  


        public static void Main()  
        {  
            Cliente cliente = new Cliente();
            cliente.Inicia();  
            cliente.controlador = new Controlador.ControladorVista(cliente);
            cliente.controlador.PideNombre();
            while(true);
        }  

        public Cliente() {
            controlador = new Controlador.ControladorVista(this);
        }
  
        public void Inicia()  
        {    
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
                    controlador.Error("ArgumentNullException : " + ane.ToString());  
                    Environment.Exit(0);
                }  
                catch (SocketException se)  
                {  
                    controlador.Error("SocketException : " + se.ToString()); 
                    Environment.Exit(0); 
                }  
                catch (Exception e)  
                {  
                    controlador.Error("Unexpected exception : " + e.ToString()); 
                    Environment.Exit(0); 
                }  
  
            }  
            catch (Exception e)  
            {  
                controlador.Error("Ocurrió un error: " + e.ToString());  
                Environment.Exit(0);
            }  
        }  

        //le pone nombre al usuario
        public void Identifica(String nombre) {

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("type", "IDENTIFY");
            dic.Add("message", nombre);
            String mensaje = JsonConvert.SerializeObject(dic);
            try {
                enchufe.Send(Parser.CadenaABytes(mensaje));
            } catch(SocketException se) {
                controlador.Error("Ocurrió un error al conectarse con el servidor " + se);
                enchufe.Close();
                Environment.Exit(0);
            }
            enchufe.Receive(bytes, 1024, 0);
            mensaje = Parser.BytesACadena(bytes);
            Dictionary<String, String> Json = JsonConvert.DeserializeObject<Dictionary<String, String>>(mensaje);
            if (Json != null) {
                if (Json["type"] == "INFO") {
                    controlador.Mensaje("Nombre aceptado");
                    return;
                } else if (Json["type"] == "WARNING"){
                    controlador.Mensaje("Error: " + Json["message"]);
                    controlador.PideNombre();
                }
            } else {
                controlador.Error("Ocurrió un error con el servidor");
                enchufe.Close();
                Environment.Exit(0);
            }
        }
    } 
}