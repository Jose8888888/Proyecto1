using System;
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Controlador;


namespace Servidor {
    public class Servidor  
    {  
        private static IPHostEntry host = Dns.GetHostEntry("localhost");  
        private static IPAddress ipAddress = host.AddressList[0];  
        private static IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);    
        private Socket escucha = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
        private Socket cliente;
        private Usuario usuario = new Usuario(); 
        private static List<Usuario> usuarios = new List<Usuario>();
        private Controlador.ControladorVista controlador = new ControladorVista();

  	        
        public static void Main()
        {   
            Servidor servidor = new Servidor();
            servidor.Inicia();    
            while(true) { 
                servidor.ConectaCliente();
                servidor.Escucha();
            }
        }

        public void Inicia()  
        {  
           
      
            try {   
  
                escucha = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  
                escucha.Bind(localEndPoint);  
                escucha.Listen(10);  
  
                controlador.Mensaje("Esperando conexión...");  
                

                
            }  
            catch (Exception e)  
            {  
                controlador.Mensaje(e.ToString());  
            }  
  
        }   
        
        //escucha al cliente y hace lo que le pide
        public void Escucha() {
            
            byte[] bytes;
            bytes = new byte[2048];  
            int bytesRec = cliente.Receive(bytes); 
            String msj = Encoding.UTF8.GetString(bytes, 0, bytesRec);
            Dictionary<String, String> Json = JsonConvert.DeserializeObject<Dictionary<String, String>>(msj);

            switch(Json["type"]) {
                case "IDENTIFY": 
                    String mensaje = IdentificaUsuario(Json["message"]);
                    cliente.Receive(CadenaABytes(mensaje));                   
                    break;

            }
            cliente.Close();
        }       

        //convierte una cadena en un arreglo de bytes para mandarlo por el enchufe
        private byte[] CadenaABytes(String cadena) {
            return Encoding.UTF8.GetBytes(cadena);
        }

        //le pone nombre a un usuario si es válido, si no regresa el mensaje de error
        private String IdentificaUsuario(String nombre) {
            if(!usuario.GetNombre().Equals("")) {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("type", "WARNING");
                dic.Add("message", "El usuario ya está identificado");

                return JsonConvert.SerializeObject(dic);
            } else {
                foreach(Usuario usuario in usuarios) {
                    if(nombre.Equals(usuario.GetNombre())) {
                        Dictionary<string, string> dic2 = new Dictionary<string, string>();
                        dic2.Add("type", "WARNING");
                        dic2.Add("message", "El identificador ya está siendo utilizado por otro usuario");
                        return JsonConvert.SerializeObject(dic2);
                    }
                }
                usuario.SetNombre(nombre);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("type", "INFO");
                dic.Add("message", "success");
                return JsonConvert.SerializeObject(dic);
                
            }
        }

        //espera hasta que se conecte un cliente
        public void ConectaCliente() {
            cliente = escucha.Accept();  
            usuarios.Add(usuario);
            controlador.Mensaje("Cliente recibido");  
        }
    }
}
