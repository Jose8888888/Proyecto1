using System;
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Threading;


namespace Chat {
    public class Servidor  
    {  
        private static IPHostEntry host = Dns.GetHostEntry("localhost");  
        private static IPAddress ipAddress = host.AddressList[0]; 
        private static IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1234);  
        private Socket servidor = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
        private static Dictionary<Socket, Usuario> usuarios = new Dictionary<Socket, Usuario>();
        private static Dictionary<Usuario, Socket> enchufes = new Dictionary<Usuario, Socket>();

        private ControladorVista controlador = new ControladorVista();

  	        
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
            bool estaActivo = true;

            while(estaActivo) {
                Dictionary<String, String> json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Recibe(cliente));
                if (json != null) {
                    AnalizaJson(json, cliente);
                } else {
                    controlador.Error("Ocurrió un error con el cliente");
                    cliente.Close();
                    estaActivo = false;
                }
            }
        }       

        

        //le pone nombre a un usuario si es válido, si no regresa el mensaje de error
        private String IdentificaUsuario(String nombre, Usuario cliente) {
            if(!cliente.GetNombre().Equals("")) {
                Dictionary<string, string> json = new Dictionary<string, string>();
                json.Add("type", "WARNING");
                json.Add("message", "El usuario ya está identificado");

                return JsonConvert.SerializeObject(json);
            } else {
                foreach(Usuario usuario in usuarios.Values) {
                    if(nombre.Equals(usuario.GetNombre())) {
                        Dictionary<string, string> json2 = new Dictionary<string, string>();
                        json2.Add("type", "WARNING");
                        json2.Add("message", "El usuario '" + nombre + "' ya existe");
                        return JsonConvert.SerializeObject(json2);
                    }
                }
                cliente.SetNombre(nombre);
                Dictionary<string, string> json = new Dictionary<string, string>();
                json.Add("type", "INFO");
                json.Add("message", "success");
                return JsonConvert.SerializeObject(json);
                
            }
        }

        //espera hasta que se conecte un cliente
        private void ConectaCliente() {
            Socket cliente;
            try {
                cliente = servidor.Accept();  
            } catch(SocketException se) {
                controlador.Error("Ocurrió un error con el cliente");
                ConectaCliente();
                return;
            }
            Usuario usuario = new Usuario();
            usuarios.Add(cliente, usuario);
            enchufes.Add(usuario, cliente);
            controlador.Mensaje("Cliente recibido");  
            Thread hilo = new Thread(ConectaCliente);
            hilo.Start();
            Escucha(cliente);
        }

        //envía un mensaje privado a un usuario
        private void EnviaMensaje(Usuario destinatario, String mensaje, Usuario emisor) {
            Dictionary<string,string> json = new Dictionary<string, string>();
            json.Add("type", "MESSAGE_FROM");
            json.Add("username", emisor.GetNombre());
            json.Add("message", mensaje);
            String mensajeJson = JsonConvert.SerializeObject(json);
            Envia(enchufes[destinatario], Parser.CadenaABytes(mensajeJson));
            
        }

        //envía un mensaje al cliente por el enchufe
        private void Envia(Socket cliente, byte[] mensaje) {
            try {
                lock(cliente)
                    cliente.Send(mensaje, 1024, 0);
            } catch(Exception) {
                controlador.Error("Ocurrió un error al conectarse con el cliente");
                cliente.Close();
                Environment.Exit(0);
            }
        }

        //recibe un mensaje del enchufe del cliente
        private String Recibe(Socket cliente) {
            byte[] bytes = new byte[1024];
            try {
                cliente.Receive(bytes, 1024, 0);
            } catch(SocketException se) {
                controlador.Error("Ocurrió un error al conectarse con el servidor " + se);
                cliente.Close();
                Environment.Exit(0);
            }

            return Parser.BytesACadena(bytes);
        }

        //analiza un mensaje Json
        private void AnalizaJson(Dictionary<string, string> json, Socket cliente) {
            switch(json["type"]) {
                        case "IDENTIFY": 
                            String mensaje = IdentificaUsuario(json["message"], usuarios[cliente]);
                            Envia(cliente, Parser.CadenaABytes(mensaje));  
                            AvisaNuevoUsuario(usuarios[cliente]);                                                   
                            break;
                        
                        case "MESSAGE":
                            Usuario usuario = null;
                            foreach (Usuario u in usuarios.Values) {
                                if (u.GetNombre() == json["username"]) {
                                    usuario = u;
                                    break;
                                }
                                
                            }
                            if (usuario != null) {
                                EnviaMensaje(usuario, json["message"], usuarios[cliente]);
                                
                            } else {
                                Dictionary<string, string> json2 = new Dictionary<string, string>();
                                json2.Add("type", "WARNING");
                                json2.Add("message", "El usuario '" + json["username"] + "' no existe");
                                mensaje = JsonConvert.SerializeObject(json2);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                            }
                            break;
                        
                        case "STATUS":
                            Usuario.Estado estado = (Usuario.Estado) Enum.Parse(typeof(Usuario.Estado), json["status"]);
                            
                            mensaje = CambiaEstado(usuarios[cliente], estado);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            AvisaNuevoEstado(usuarios[cliente], json["status"]);
                            break;
                        
                        case "USERS":
                            Dictionary<string, string> json3 = new Dictionary<string, string>();
                            json3.Add("type", "USER_LIST");
                            json3.Add("usernames", Nombres());
                            mensaje = JsonConvert.SerializeObject(json3);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            break;
                    }
        }

        //avisa de un nuevo usuario a los demás usuarios
        private void AvisaNuevoUsuario(Usuario nuevoUsuario) {
            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "NEW_USER");
            json.Add("username", nuevoUsuario.GetNombre());
            String mensaje = JsonConvert.SerializeObject(json);
            foreach(Usuario usuario in usuarios.Values) {
                if (usuario != nuevoUsuario) {
                    Envia(enchufes[usuario], Parser.CadenaABytes(mensaje));
                }
            }
        }

        //cambia el estado de un usuario, si ya tenía ese estado regresa un error
        private String CambiaEstado(Usuario usuario, Usuario.Estado estado) {
            Dictionary<string, string> json = new Dictionary<string, string>();
            if (usuario.GetEstado() != estado) {
                usuario.SetEstado(estado);
                json.Add("type", "INFO");
                json.Add("message", "success");
                return JsonConvert.SerializeObject(json);
            } else {
                json.Add("type", "WARNING");
                json.Add("message", "El estado ya es " + estado);
                return JsonConvert.SerializeObject(json);
            }
        }

        //avisa a los demás usuarios que un usuario cambió de estado
        private void AvisaNuevoEstado(Usuario usuario, String estado) {
            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "NEW_STATUS");
            json.Add("username", usuario.GetNombre());
            json.Add("status", estado);
            String mensaje = JsonConvert.SerializeObject(json);
            foreach(Usuario u in usuarios.Values) {
                if (u != usuario) {
                    Envia(enchufes[u], Parser.CadenaABytes(mensaje));
                }
            }
        }

        //regresa una lista Json de los nombres de los usuarios
        private String Nombres() {
            List<string> nombres = new List<string>();
            foreach (Usuario usuario in usuarios.Values) {
                nombres.Add(usuario.GetNombre());
            }
            return JsonConvert.SerializeObject(nombres);
        }
    }
}
