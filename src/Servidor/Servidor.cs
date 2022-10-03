using System;
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Controlador;
using System.Threading;


namespace Servidor {
    public class Servidor  
    {  
        private static IPHostEntry host = Dns.GetHostEntry("localhost");  
        private static IPAddress ipAddress = host.AddressList[0]; 
        private static IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1234);  
        private Socket servidor = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
        private static Dictionary<Socket, Usuario> usuarios = new Dictionary<Socket, Usuario>();
        private Controlador.ControladorVista controlador = new ControladorVista();
        bool estaActivo = true;

  	        
        public static void Main()
        {   
            Servidor servidor = new Servidor();
            servidor.Inicia();    
            
        }

        public void Inicia()  
        {  
           
      
            try {   
  
                servidor = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  
                servidor.Bind(localEndPoint);  
                servidor.Listen(10);  
  
                controlador.Mensaje("Esperando conexión...");  
                

                
            }  
            catch (Exception e)  
            {  
                controlador.Error("Ocurrió un error: " + e.ToString());  
            }  

            ConectaCliente();

        }   
        
        //recibe información del cliente
        public void Escucha(Socket cliente) {
            
            while(estaActivo) {
                byte[] bytes;
                bytes = new byte[1024];  
                cliente.Receive(bytes); 
                String msj = Parser.BytesACadena(bytes);
                Dictionary<String, String> Json = JsonConvert.DeserializeObject<Dictionary<String, String>>(msj);
                if (Json != null) {
                    switch(Json["type"]) {
                        case "IDENTIFY": 
                            String mensaje = IdentificaUsuario(Json["message"], usuarios[cliente]);
                            cliente.Send(Parser.CadenaABytes(mensaje));                   
                            break;

                    }
                } else {
                    controlador.Error("Ocurrió un error con el cliente");
                    Desconecta(cliente);
                }
            }
        }       

        

        //le pone nombre a un usuario si es válido, si no regresa el mensaje de error
        private String IdentificaUsuario(String nombre, Usuario cliente) {
            if(!cliente.GetNombre().Equals("")) {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("type", "WARNING");
                dic.Add("message", "El usuario ya está identificado");

                return JsonConvert.SerializeObject(dic);
            } else {
                foreach(Usuario usuario in usuarios.Values) {
                    if(nombre.Equals(usuario.GetNombre())) {
                        Dictionary<string, string> dic2 = new Dictionary<string, string>();
                        dic2.Add("type", "WARNING");
                        dic2.Add("message", "El identificador ya está siendo utilizado por otro usuario");
                        return JsonConvert.SerializeObject(dic2);
                    }
                }
                cliente.SetNombre(nombre);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("type", "INFO");
                dic.Add("message", "success");
                return JsonConvert.SerializeObject(dic);
                
            }
        }

        //espera hasta que se conecte un cliente
        public void ConectaCliente() {
            Socket cliente;
            try {
                cliente = servidor.Accept();  
            } catch(SocketException se) {
                controlador.Error("Ocurrió un error con el cliente");
                ConectaCliente();
                return;
            }
            usuarios.Add(cliente, new Usuario());
            controlador.Mensaje("Cliente recibido");  
            Thread hilo = new Thread(ConectaCliente);
            hilo.Start();
            Escucha(cliente);
        }

        //desconecta al cliente
        private void Desconecta(Socket cliente) {
            cliente.Close();
            estaActivo = false;
         }
    }
}
