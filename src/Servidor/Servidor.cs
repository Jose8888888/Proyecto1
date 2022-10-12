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
        private static IPHostEntry host;
        private static IPAddress ipAddress; 
        private static IPEndPoint localEndPoint;  
        private Socket servidor; 
        private Dictionary<Socket, Usuario> usuarios = new Dictionary<Socket, Usuario>();
        private Dictionary<Usuario, Socket> enchufes = new Dictionary<Usuario, Socket>();
        private static ControladorVista controlador = new ControladorVista();
        private List<Cuarto> cuartos = new List<Cuarto>();

  	        
        public static void Main()
        {   
            String IP = controlador.PideIP();
            int puerto = controlador.PidePuerto();
            Servidor servidor = new Servidor("localhost", puerto);
            servidor.Inicia();    
            
        }

        public Servidor(String IP, int puerto) {
            host = Dns.GetHostEntry(IP);  
            ipAddress = host.AddressList[0]; 
            localEndPoint = new IPEndPoint(ipAddress, puerto);  
            servidor = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
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
                Environment.Exit(0);
            }  

            ConectaCliente();

        }   
        
        //recibe información del cliente
        public void Escucha(Socket cliente) {
            while(cliente.Connected) {
                Dictionary<String, String> json;
                try {
                    json = JsonConvert.DeserializeObject<Dictionary<String, String>>(Recibe(cliente));
                } catch (Exception e) {
                    json = new Dictionary<string, string>();
                    json.Add("type", "ERROR");
                    json.Add("message", "El mensaje no es válido");
                    String mensaje = JsonConvert.SerializeObject(json);
                    Envia(cliente, Parser.CadenaABytes(mensaje));
                    DesconectaUsuario(usuarios[cliente]);
                    return;
                }
                if (json != null) {
                    if (json["type"] == "IDENTIFY" || usuarios[cliente].GetNombre() != null) {
                        AnalizaJson(json, cliente);
                    } else {
                        json.Clear();
                        json.Add("type", "ERROR");
                        json.Add("message", "No te has identificado");
                        String mensaje = JsonConvert.SerializeObject(json);
                        Envia(cliente, Parser.CadenaABytes(mensaje));
                        controlador.Error("Ocurrió un error con un cliente");
                        DesconectaUsuario(usuarios[cliente]);
                        return;
                    }
                } else {
                    json = new Dictionary<string, string>();
                    json.Add("type", "ERROR");
                    json.Add("message", "El mensaje no es válido");
                    String mensaje = JsonConvert.SerializeObject(json);
                    Envia(cliente, Parser.CadenaABytes(mensaje));
                    DesconectaUsuario(usuarios[cliente]);
                    return;
                }
            }
        }       

        

        //le pone nombre a un usuario si es válido, si no regresa el mensaje de error
        private String IdentificaUsuario(String nombre, Usuario cliente) {
            if(cliente.GetNombre() != null) {
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
                controlador.Error("Ocurrió un error con un cliente");
                cliente.Close();
            }
        }

        //recibe un mensaje del enchufe del cliente
        private String Recibe(Socket cliente) {
            byte[] bytes = new byte[1024];
            try {
                cliente.Receive(bytes, 1024, 0);
            } catch(SocketException se) {
                controlador.Error("Ocurrió un error con un cliente " + se);
                DesconectaUsuario(usuarios[cliente]);
            }

            return Parser.BytesACadena(bytes);
        }

        //analiza un mensaje Json
        private void AnalizaJson(Dictionary<string, string> json, Socket cliente) {
            Dictionary<string, string> nuevoJson = new Dictionary<string, string>();
            String mensaje;
            Cuarto cuarto;
            switch(json["type"]) {
                    case "IDENTIFY": 
                        if (json.ContainsKey("username")) {
                            mensaje = IdentificaUsuario(json["username"], usuarios[cliente]);
                            Envia(cliente, Parser.CadenaABytes(mensaje));  
                        } else {
                            nuevoJson.Add("type", "ERROR");
                            nuevoJson.Add("message", "El mensaje está incompleto");
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            DesconectaUsuario(usuarios[cliente]);
                        }
                        break;
                        
                    case "MESSAGE":
                        if (json.ContainsKey("username") && json.ContainsKey("message")) {
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
                                nuevoJson.Add("operation", "MESSAGE");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                            }
                        } else {
                            nuevoJson.Add("type", "ERROR");
                            nuevoJson.Add("message", "El mensaje está incompleto");
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            DesconectaUsuario(usuarios[cliente]);
                        }
                        break;
                        
                    case "STATUS":
                        if (json.ContainsKey("status") && (json["status"] == "ACTIVE" || json["status"] == "AWAY" || json["status"] == "BUSY")) {
                            Usuario.Estado estado = (Usuario.Estado) Enum.Parse(typeof(Usuario.Estado), json["status"]);
                            
                            mensaje = CambiaEstado(usuarios[cliente], estado);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            
                        } else {
                            nuevoJson.Add("type", "ERROR");
                            nuevoJson.Add("message", "El mensaje está incompleto");
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            DesconectaUsuario(usuarios[cliente]);
                        }
                        break;
            
                        
                        case "USERS":
                            nuevoJson.Add("type", "USER_LIST");
                            nuevoJson.Add("usernames", Nombres());
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                        break;

                    case "PUBLIC_MESSAGE":
                        if (json.ContainsKey("message")) {
                            EnviaMensaje(json["message"], usuarios[cliente]);
                        } else {
                            nuevoJson.Add("type", "ERROR");
                            nuevoJson.Add("message", "El mensaje está incompleto");
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            DesconectaUsuario(usuarios[cliente]);
                        }
                        break;

                    case "NEW_ROOM":
                        if (json.ContainsKey("roomname")) {
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
                                
                        } else {
                            nuevoJson.Add("type", "ERROR");
                            nuevoJson.Add("message", "El mensaje está incompleto");
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            DesconectaUsuario(usuarios[cliente]);
                        }
                        break;

                    case "INVITE":
                        if (json.ContainsKey("usernames") && json.ContainsKey("roomname")) {
                            cuarto = BuscaCuarto(json["roomname"]);
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
                        } else {
                            nuevoJson.Add("type", "ERROR");
                            nuevoJson.Add("message", "El mensaje está incompleto");
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            DesconectaUsuario(usuarios[cliente]);
                        }
                        break;

                    case "JOIN_ROOM":
                        if (json.ContainsKey("roomname")) {
                            cuarto = BuscaCuarto(json["roomname"]);
                            if (cuarto == null) {
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "El cuarto '" + json["roomname"] + "' no existe.");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                                break;
                            } else if (usuarios[cliente].EstaEnCuarto(cuarto)) {
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "El usuario ya se unió al cuarto '" + json["roomname"] + "'");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                                return;
                            } else if (usuarios[cliente].EstaInvitado(cuarto)) {
                                nuevoJson.Add("type", "INFO");
                                nuevoJson.Add("message", "success");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                                
                                nuevoJson.Clear();
                                nuevoJson.Add("type", "JOINED_ROOM");
                                nuevoJson.Add("roomname", json["roomname"]);
                                nuevoJson.Add("username", usuarios[cliente].GetNombre());
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                foreach (Usuario miembro in cuarto.GetMiembros()) {
                                    Envia(enchufes[miembro], Parser.CadenaABytes(mensaje));
                                }
                                usuarios[cliente].AgregaCuarto(cuarto);
                                cuarto.AgregaMiembro(usuarios[cliente]);
                                
            
                            } else {
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "El usuario no ha sido invitado al cuarto '" + json["roomname"] + "'");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                                return;
                            }
                        } else {
                            nuevoJson.Add("type", "ERROR");
                            nuevoJson.Add("message", "El mensaje está incompleto");
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            DesconectaUsuario(usuarios[cliente]);
                        }
                        break;

                    case "ROOM_USERS":
                        if (json.ContainsKey("roomname")) {
                            cuarto = BuscaCuarto(json["roomname"]);
                            if (cuarto == null) {
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "El cuarto '" + json["roomname"] + "' no existe");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                            } else if (usuarios[cliente].EstaEnCuarto(cuarto)) {
                                List<String> miembros = new List<String>();
                                foreach (Usuario miembro in cuarto.GetMiembros()) {
                                    miembros.Add(miembro.GetNombre());
                                }

                                nuevoJson.Add("type", "ROOM_USER_LIST");
                                nuevoJson.Add("usernames", JsonConvert.SerializeObject(miembros));
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                            } else {
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "El usuario no se ha unido al cuarto '" + json["roomname"] + "'");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                            }
                        } else {
                            nuevoJson.Add("type", "ERROR");
                            nuevoJson.Add("message", "El mensaje está incompleto");
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            DesconectaUsuario(usuarios[cliente]);
                        }

                        break;

                    case "ROOM_MESSAGE":
                        if (json.ContainsKey("roomname") && json.ContainsKey("message")) {
                            cuarto = BuscaCuarto(json["roomname"]);
                            if (cuarto == null) {
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "El cuarto '" + json["roomname"] + "' no existe");
                                nuevoJson.Add("operation", "ROOM_MESSAGE");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);

                                Envia(cliente, Parser.CadenaABytes(mensaje));
                            } else if (usuarios[cliente].EstaEnCuarto(cuarto)) {

                                nuevoJson.Add("type", "ROOM_MESSAGE_FROM");
                                nuevoJson.Add("roomname", json["roomname"]);
                                nuevoJson.Add("username", usuarios[cliente].GetNombre());
                                nuevoJson.Add("message", json["message"]);
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                foreach (Usuario u in cuarto.GetMiembros()) {
                                    if (u != usuarios[cliente]) {
                                        Envia(enchufes[u], Parser.CadenaABytes(mensaje));
                                    }
                                }
                            } else {
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "El usuario no se ha unido al cuarto '" + json["roomname"] + "'");
                                nuevoJson.Add("operation", "ROOM_MESSAGE");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                            }
                        } else {
                            nuevoJson.Add("type", "ERROR");
                            nuevoJson.Add("message", "El mensaje está incompleto");
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            DesconectaUsuario(usuarios[cliente]);
                        }
                        break;

                    case "LEAVE_ROOM":
                        if (json.ContainsKey("roomname")) {
                            cuarto = BuscaCuarto(json["roomname"]);
                            if (cuarto == null) {
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "El cuarto '" + json["roomname"] + "' no existe");
                                nuevoJson.Add("operation", "ROOM_MESSAGE");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);

                                Envia(cliente, Parser.CadenaABytes(mensaje));
                            } else if (usuarios[cliente].EstaEnCuarto(cuarto)) {
                                SacaDeCuarto(usuarios[cliente], cuarto);

                                nuevoJson.Add("type", "INFO");
                                nuevoJson.Add("message", "success");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                                
                            } else {
                                nuevoJson.Add("type", "WARNING");
                                nuevoJson.Add("message", "El usuario no se ha unido al cuarto '" + json["roomname"] + "'");
                                nuevoJson.Add("operation", "ROOM_MESSAGE");
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                Envia(cliente, Parser.CadenaABytes(mensaje));
                            }
                        } else {
                            nuevoJson.Add("type", "ERROR");
                            nuevoJson.Add("message", "El mensaje está incompleto");
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            DesconectaUsuario(usuarios[cliente]);
                        }
                        break;

                        case "DISCONNECT":
                            String nombreUsuario = usuarios[cliente].GetNombre();
                            nuevoJson.Add("type", "LEFT_ROOM");
                            nuevoJson.Add("username", nombreUsuario);
                            List<Cuarto> listaCuartos = usuarios[cliente].GetCuartos();
                            DesconectaUsuario(usuarios[cliente]);
                            foreach (Cuarto c in listaCuartos) {
                                nuevoJson.Add("roomname", c.GetNombre());
                                mensaje = JsonConvert.SerializeObject(nuevoJson);
                                foreach (Usuario miembro in c.GetMiembros()) {
                                     Envia(enchufes[miembro], Parser.CadenaABytes(mensaje));
                                }
                            }

                            nuevoJson.Clear();
                            nuevoJson.Add("type", "DISCONNECTED");
                            nuevoJson.Add("username", nombreUsuario);
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            foreach (Usuario u in usuarios.Values) {
                                     Envia(enchufes[u], Parser.CadenaABytes(mensaje));
                                }
                               
                            break;

                        default:
                            nuevoJson.Add("type", "ERROR");
                            nuevoJson.Add("message", "El mensaje no es válido");
                            mensaje = JsonConvert.SerializeObject(nuevoJson);
                            Envia(cliente, Parser.CadenaABytes(mensaje));
                            DesconectaUsuario(usuarios[cliente]);
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
                AvisaNuevoEstado(usuario, Enum.GetName(typeof(Usuario.Estado), estado));
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

        //regresa el cuarto con el nombre que recibe
        private Cuarto BuscaCuarto(String nombre) {
            Cuarto cuarto = null;
            foreach (Cuarto c in cuartos) {
                if (c.GetNombre() == nombre) {
                    cuarto = c;
                }
            }
            return cuarto;
        }

        //desconecta a un usuario
        private void DesconectaUsuario(Usuario usuario) {
            enchufes[usuario].Close();
            foreach (Cuarto cuarto in cuartos) {
                if (usuario.EstaEnCuarto(cuarto)) {
                    SacaDeCuarto(usuario, cuarto);
                }
            }
            usuarios.Remove(enchufes[usuario]);
            enchufes.Remove(usuario);
            
        }

        //Saca a un usuario de un cuarto
        private void SacaDeCuarto(Usuario usuario, Cuarto cuarto) {
            cuarto.EliminaMiembro(usuario);
            usuario.EliminaCuarto(cuarto);

            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "LEFT_ROOM");
            json.Add("roomname", cuarto.GetNombre());
            json.Add("username", usuario.GetNombre());
            String mensaje = JsonConvert.SerializeObject(json);
            if (cuarto.GetMiembros().Any()) {
                foreach (Usuario u in cuarto.GetMiembros()) {
                    Envia(enchufes[u], Parser.CadenaABytes(mensaje));
                }
            } else {
                cuartos.Remove(cuarto);
            }
        }
    }
}
