using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Text.Json;
using Newtonsoft.Json;

  
namespace Chat {
    public class Cliente  
    {  
        private static IPHostEntry host = Dns.GetHostEntry("localhost");  
        private static IPAddress ipAddress = host.AddressList[0];  
        private IPEndPoint remoteEP = new IPEndPoint(ipAddress, 1234);  
  
        private static Socket enchufe = new Socket(ipAddress.AddressFamily,  
                    SocketType.Stream, ProtocolType.Tcp);  
        private ControladorVista controlador;
        private List<Usuario> usuarios = new List<Usuario>();


        public static void Main()  
        {  
            Cliente cliente = new Cliente();
            cliente.Inicia();  
            cliente.controlador = new ControladorVista(cliente);
            cliente.controlador.PideNombre();
            while(true) {
                cliente.AnalizaMensaje(cliente.controlador.Escucha());
            };
        }  

        public Cliente() {
            controlador = new ControladorVista(this);
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

            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "IDENTIFY");
            json.Add("message", nombre);
            String mensaje = JsonConvert.SerializeObject(json);
            Envia(Parser.CadenaABytes(mensaje));
            Dictionary<String, String> Json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Recibe());
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

        //revisa si un mensaje es público, privado, etc.
        private void AnalizaMensaje(String mensaje) {
            String[] separados = mensaje.Split(": ", 2);
            foreach(Usuario usuario in usuarios) {
                if (usuario.GetNombre() == separados[0]) {

                    EnviaMensaje(separados[0], separados[1]);
                    return;
                }
            }
            controlador.Error("El usuario '" + separados[0] + "' no existe");
        }

        //envía un mensaje privado
        private void EnviaMensaje(String destinatario, String mensaje) {
            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "MESSAGE");
            json.Add("username", destinatario);
            json.Add("message", mensaje);
            String mensajeJson = JsonConvert.SerializeObject(json);
            try {
                Envia(Parser.CadenaABytes(mensajeJson));
            } catch(SocketException se) {
                controlador.Error("Ocurrió un error al conectarse con el servidor " + se);
                enchufe.Close();
                Environment.Exit(0);
            }
        }

        //envía un mensaje al servidor por el enchufe
        private void Envia(byte[] mensaje) {
            try {
                enchufe.Send(mensaje, 1024, 0);
            } catch(SocketException se) {
                controlador.Error("Ocurrió un error al conectarse con el servidor " + se);
                enchufe.Close();
                Environment.Exit(0);
            }
        }

        //recibe un mensaje del enchufe del servidor
        private String Recibe() {
            byte[] bytes = new byte[1024];
            try {
                enchufe.Receive(bytes, 1024, 0);
            } catch(SocketException se) {
                controlador.Error("Ocurrió un error al conectarse con el servidor " + se);
                enchufe.Close();
                Environment.Exit(0);
            }

            return Parser.BytesACadena(bytes);
        }
    } 
}