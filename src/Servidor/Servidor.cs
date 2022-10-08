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
        private List<Cuarto> cuartos = new List<Cuarto>();

  	        
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
                        Dictionary<string, string> nuevoJson = new Dictionary<string, string>();
                        nuevoJson.Add("type", "WARNING");
                        nuevoJson.Add("message", "El usuario '" + nombre + "' ya existe");
                        return JsonConvert.SerializeObject(nuevoJson);
                    }
                }
                cliente.SetNombre(nombre);
                Dictionary<string, string> json = new Dictionary<string, string>();
                json.Add("type", "INFO");
                json.Add("message", "success");
                AvisaNuevoUsuario(usuarios[enchufes[cliente]]);                                                   
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

        //envía un mensaje público
        private void EnviaMensaje(String mensaje, Usuario emisor) {
            Dictionary<string,string> json = new Dictionary<string, string>();
            json.Add("type", "PUBLIC_MESSAGE_FROM");
            json.Add("username", emisor.GetNombre());
            json.Add("message", mensaje);
            String mensajeJson = JsonConvert.SerializeObject(json);
            foreach (Usuario usuario in usuarios.Values) {
                if (usuario != emisor) {
                    Envia(enchufes[usuario], Parser.CadenaABytes(mensajeJson));
                }
            }
        }

        //envía un mensaje al cliente por el enchufe
        private void Envia(Socket cliente, byte[] mensaje) {
            try {
                lock(cliente)
                    cliente.Send(mensaje, 1024, 0);
            } catch(Exception) {
                controlador.Error("Ocurrió un error al conectarse con el cliente");
                cliente.Close();
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
            }

            return Parser.BytesACadena(bytes);
        }

        //analiza un mensaje Json
        private void AnalizaJson(Dictionary<string, string> json, Socket cliente) {
            Dictionary<string, string> nuevoJson = new Dictionary<string, string>();
            switch(json["type"]) {
                        case "IDENTIFY": 
                            String mensaje = IdentificaUsuario(json["message"], usuarios[cliente]);
                            Envia(cliente, Parser.CadenaABytes(mensaje));  
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
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "El usuario '" + json["username"] + "' no existe");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
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
                            nuevoJson.Add("type", "USER_LIST");
                            nuevoJson.Add("usernames", Nombres());
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            break;

                        case "PUBLIC_MESSAGE":
                            
                            EnviaMensaje(json["message"], usuarios[cliente]);
                            break;

                        case "NEW_ROOM":
                            foreach (Cuarto c in cuartos) {
                                if (c.GetNombre() == json["roomname"]) {
                                    nuevoJson.Add("type", "WARNING");
                                    nuevoJson.Add("message", "El cuarto " + json["roomname"] + " ya existe.");
                                    mensaje = JsonConvert.SerializeObject(nuevoJson);
                                    Envia(cliente, Parser.CadenaABytes(mensaje));
                                    return;
                                    
                                } 
                            }
                                    Cuarto nuevoCuarto = new Cuarto(json["roomname"], usuarios[cliente]);
                                    cuartos.Add(nuevoCuarto);

                                    nuevoJson.Add("type", "INFO");
                                    nuevoJson.Add("message", "succes");
                                    mensaje = JsonConvert.SerializeObject(nuevoJson);
                                    Envia(cliente, Parser.CadenaABytes(mensaje));
                                
                            
                            break;

                        case "INVITE":
                            Cuarto cuarto = null;
                            foreach (Cuarto c in cuartos) {
                                if (c.GetNombre() == json["roomname"]) {
                                    cuarto = c;
                                }
                            }
                            if (cuarto == null) {
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "El cuarto '" + json["roomname"] + "' no existe.");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                                break;
                            } else if (usuarios[cliente].EstaEnCuarto(cuarto)) {
                                List<Usuario> invitados = new List<Usuario>();
                                List<string> nombres = JsonConvert.DeserializeObject<List<string>>(json["usernames"]);
                                int numInvitados = 0;
                                foreach (String nombre in nombres) {
                                    foreach (Usuario u in usuarios.Values) {
                                        if (u.GetNombre() == nombre) {
                                            invitados.Add(u);
                                            break;
                                        }
                                    }
                                    if (invitados.Count == numInvitados) {
                                        nuevoJson.Add("type", "WARNING");
                                        nuevoJson.Add("message", "El usuario " + nombre + " no existe.");
                                        mensaje = JsonConvert.SerializeObject(nuevoJson);
                                        Envia(cliente, Parser.CadenaABytes(mensaje));
                                        return;
                                    }
                                    numInvitados++;
                                }
                            

                                nuevoJson.Add("type", "INFO");
                                nuevoJson.Add("message", "success");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));

                                nuevoJson.Clear();
                                nuevoJson.Add("type", "INVITATION");
                                nuevoJson.Add("message", usuarios[cliente].GetNombre() + " te invita al cuarto '" + json["roomname"] + "'");
                                nuevoJson.Add("username", usuarios[cliente].GetNombre());
                                nuevoJson.Add("roomname", json["roomname"]);
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                foreach (Usuario invitado in invitados) {
                                    invitado.AgregaInvitacion(cuarto);
                                    Envia(enchufes[invitado], Parser.CadenaABytes(mensaje));
                                }
                            } else {
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "No estás en el cuarto '" + json["roomname"] + "'");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                                break;
                            }

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
