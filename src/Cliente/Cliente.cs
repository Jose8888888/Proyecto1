using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Text.Json;
using Newtonsoft.Json;
using System.Threading;


  
namespace Chat {
    public class Cliente  
    {  
        #pragma warning disable CS8600
        #pragma warning disable CS8618
        private static IPHostEntry host;  
        private static IPAddress ipAddress;  
        private IPEndPoint remoteEP;  
  
        private static Socket enchufe;  
        private ControladorVista controlador;
        private String guardado = "";
        private bool puedeEscuchar = true;
        private bool estaEscuchando = false;
        private bool estaActivo = true;


        public static void Main()  
        {  
            ControladorVista controlador = new ControladorVista();
            String IP = controlador.PideIP();
            int puerto = controlador.PidePuerto();
            Cliente cliente = new Cliente(IP, puerto);
            cliente.Inicia();  
            Thread hilo = new Thread(cliente.Escucha);
            hilo.Start();
            cliente.controlador = new ControladorVista(cliente);
            cliente.controlador.PideNombre();

            while(true) {
                cliente.AnalizaMensaje(cliente.controlador.Escucha());
                cliente.puedeEscuchar = true;
            }
        }  

        public Cliente(String IP, int puerto) {
            controlador = new ControladorVista(this);

            host = Dns.GetHostEntry(IP);  
            ipAddress = host.AddressList[0]; 
            remoteEP = new IPEndPoint(ipAddress, puerto);  
            enchufe = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 
        }
  
        public void Inicia()  
        {    
            try  
            {  

            
  
                try  
                {  
                    enchufe.Connect(remoteEP);  
  
                    if (enchufe.RemoteEndPoint != null)
                        controlador.Mensaje("Enchufe conectado a " + enchufe.RemoteEndPoint.ToString());  
  
                
  
                }  
                catch (ArgumentNullException)  
                {  
                    controlador.Error("ArgumentNullException");  
                    enchufe.Close();
                    Environment.Exit(0);
                }  
                catch (SocketException)  
                {  
                    controlador.Error("SocketException"); 
                    enchufe.Close();
                    Environment.Exit(0); 
                }  
                catch (Exception)  
                {  
                    controlador.Error("Unexpected exception"); 
                    enchufe.Close();
                    Environment.Exit(0);
                }  
  
            }  
            catch (Exception)  
            {  
                controlador.Error("Ocurrió un error");  
                enchufe.Close();
                Environment.Exit(0);
            }  
        }  

        //le pone nombre al usuario
        public void Identifica(String nombre) {

            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "IDENTIFY");
            json.Add("username", nombre);
            String mensaje = JsonConvert.SerializeObject(json);
            Envia(Parser.CadenaABytes(mensaje + "\n"));
            
            Dictionary<String, String> nuevoJson = JsonConvert.DeserializeObject<Dictionary<String, String>>(MensajeRecibido());
            if (nuevoJson != null) {
                if (nuevoJson["type"] == "INFO") {
                    controlador.Mensaje("Nombre aceptado");
                    return;
                } else if (nuevoJson["type"] == "WARNING"){
                    controlador.Mensaje("Error: " + nuevoJson["message"]);
                    controlador.PideNombre();
                }
            } else {
                controlador.Error("Ocurrió un error con el servidor");
                enchufe.Close();
                Environment.Exit(0);
            }
        }

        //revisa si es un comando o un mensaje público, privado, etc.
        private void AnalizaMensaje(String mensaje) {
            puedeEscuchar = false;
            if (mensaje != "" && mensaje[0] == '/') {
                String[] separados = mensaje.Split(" ", 2);
                String argumento = "";
                if(mensaje.Split(" ").Length > 1) {
                    argumento = separados[1];
                }
                AnalizaComando(separados[0].Remove(0,1), argumento);
            } else {
                String[] separados = mensaje.Split("] ", 2);
                if (mensaje != "" && mensaje[0] == '[' && separados.Length > 1) {
                    EnviaMensajeACuarto(separados[0].Remove(0,1), separados[1]);
                } else {
                    separados = mensaje.Split(": ", 2);
                    if (separados.Length == 2) {
                        EnviaMensajePrivado(separados[0], separados[1]);
                        return;
                    } else {
                        EnviaMensajePublico(mensaje);
                    }
                }
            }
            
        }

        //envía un mensaje privado
        private void EnviaMensajePrivado(String destinatario, String mensaje) {
            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "MESSAGE");
            json.Add("username", destinatario);
            json.Add("message", mensaje);
            String mensajeJson = JsonConvert.SerializeObject(json);
            Envia(Parser.CadenaABytes(mensajeJson));

        }

        //envía un mensaje público
        private void EnviaMensajePublico(String mensaje) {
            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "PUBLIC_MESSAGE");
            json.Add("message", mensaje);
            String mensajeJson = JsonConvert.SerializeObject(json);
            Envia(Parser.CadenaABytes(mensajeJson));

        }

        //envía un mensaje a un cuarto
        private void EnviaMensajeACuarto(String cuarto, String mensaje) {
            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "ROOM_MESSAGE");
            json.Add("roomname", cuarto);
            json.Add("message", mensaje);
            String mensajeJson = JsonConvert.SerializeObject(json);
            Envia(Parser.CadenaABytes(mensajeJson));

        }

        //envía un mensaje al servidor por el enchufe
        private void Envia(byte[] mensaje) {
            try {
                enchufe.Send(mensaje, mensaje.Length, 0);
                
                

            } catch(SocketException) {
                controlador.Error("Ocurrió un error al conectarse con el servidor ");
                enchufe.Close();
                Environment.Exit(0);
            }
        }

        //recibe un mensaje del enchufe del servidor
        private String Recibe() {
            byte[] bytes = new byte[1024];
            try {
                    enchufe.Receive(bytes, 1024, 0);
            } catch(SocketException) {
                controlador.Error("Ocurrió un error al conectarse con el servidor ");
                enchufe.Close();
                Environment.Exit(0);
            }
            return Parser.BytesACadena(bytes);
        }

        //escucha los mensajes del servidor
        private void Escucha() {

            while(true) {
                if (puedeEscuchar) {
                    estaEscuchando = true;
                    Dictionary<String, String> json = new Dictionary<string, string>();
                    guardado = Recibe();
                    try {
                        json = JsonConvert.DeserializeObject<Dictionary<String, String>>(guardado);
                    } catch (Newtonsoft.Json.JsonReaderException) {
                        controlador.Error("El mensaje recibido no es válido");
                        enchufe.Close();
                        Environment.Exit(0);
                    }

                    if (json != null) {
                        AnalizaJson(json);
                    } else if (estaActivo) {
                        controlador.Error("Ocurrió un error con el servidor");
                        enchufe.Close();
                        Environment.Exit(0);
                    }
                    estaEscuchando = false;
                    Thread.Sleep(10);
                }
            }
        }

        //analiza un mensaje Json
        private void AnalizaJson(Dictionary<string, string> json) {
            String mensaje;
            switch(json["type"]) {
                    case "MESSAGE_FROM": 
                        if (json.ContainsKey("username") && json.ContainsKey("message")) {
                            mensaje = "(" + json["username"] + "): " + json["message"];
                            controlador.Mensaje(mensaje);
                        } else {
                            controlador.Error("El mensaje recibido está incompleto");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                            break;

                    case "NEW_USER":
                        if (json.ContainsKey("username")) {
                            controlador.Mensaje(json["username"] + " ha entrado al chat.");
                        } else {
                            controlador.Error("El mensaje recibido está incompleto");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                        break;

                    case "NEW_STATUS":
                        if (json.ContainsKey("username") && json.ContainsKey("status")) {
                                controlador.Mensaje(json["username"] + " cambió su estado a " + json["status"]);
                        } else {
                            controlador.Error("El mensaje recibido está incompleto");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                        break;

                    case "PUBLIC_MESSAGE_FROM": 
                        if (json.ContainsKey("username") && json.ContainsKey("message")) {
                            mensaje = json["username"] + ": " + json["message"];
                            controlador.Mensaje(mensaje);
                        } else {
                            controlador.Error("El mensaje recibido está incompleto");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                        break;
                        
                    case "INVITATION":
                        if (json.ContainsKey("message")) {
                            controlador.Mensaje(json["message"]);
                        } else {
                            controlador.Error("El mensaje recibido está incompleto");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                        break;

                    case "ROOM_MESSAGE_FROM":
                        if (json.ContainsKey("username") && json.ContainsKey("message") && json.ContainsKey("roomname")) {
                            mensaje = "[" + json["roomname"] + "] " + json["username"] + ": " + json["message"];
                            controlador.Mensaje(mensaje);
                        } else {
                            controlador.Error("El mensaje recibido está incompleto");
                            enchufe.Close();
                            Environment.Exit(0);
                    }

                            break;
                    case "WARNING":
                        if (json.ContainsKey("message") && json.ContainsKey("operation") && (json["operation"] == "ROOM_MESSAGE" || json["operation"] == "MESSAGE")) {
                            controlador.Error(json["message"]);
                        }
                    break;

                    case "LEFT_ROOM":
                        if (json.ContainsKey("username") && json.ContainsKey("roomname")) {
                            mensaje = json["username"] + " ha abandonado el cuarto '" + json["roomname"] + "'";
                            controlador.Mensaje(mensaje);
                        } else {
                            controlador.Error("El mensaje recibido está incompleto");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                        break;

                    case "DISCONNECTED":
                        if (json.ContainsKey("username")) {
                            controlador.Mensaje(json["username"] + " se desconectó del chat");
                        } else {
                            controlador.Error("El mensaje recibido está incompleto");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                        break;

                    case "ERROR":
                        if (json.ContainsKey("message")) {
                            controlador.Error(json["message"]);
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                        break;

                    case "JOINED_ROOM":
                        if (json.ContainsKey("username") && json.ContainsKey("roomname")) {
                            controlador.Mensaje(json["username"] + " se unió al cuarto '" + json["roomname"] + "'");
                        } else {
                            controlador.Error("El mensaje recibido está incompleto");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                        break;

                    default:
                        if (json["type"] != "INFO" && json["type"] != "WARNING" && json["type"] != "ERROR" 
                        && json["type"] != "USER_LIST" && json["type"] != "ROOM_USER_LIST") {
                            controlador.Error("El mensaje recibido no es válido");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                        break;

                    }
        }

        //analiza un comando que recibe de la terminal
        private void AnalizaComando(String comando, String argumento) {
            Dictionary<string, string> json = new Dictionary<string, string>();
            String mensaje;
            String cuarto;
            switch(comando) {
                case "estado":
                    if (argumento == "ACTIVE" || argumento == "AWAY" || argumento == "BUSY") {
                        json.Add("type", "STATUS");
                        json.Add("status", argumento);
                        mensaje = JsonConvert.SerializeObject(json);
                        Envia(Parser.CadenaABytes(mensaje));

                        json = JsonConvert.DeserializeObject<Dictionary<String, String>>(MensajeRecibido());
                        if (json != null) {
                            if (json["type"] == "INFO") {
                                controlador.Mensaje("El estado cambió exitosamente");
                                return;
                            } else if (json["type"] == "WARNING"){
                                controlador.Mensaje("Error: " + json["message"]);
                            }
                        } else {
                            controlador.Error("Ocurrió un error con el servidor");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                    } else {
                        controlador.Error(argumento + " no es un estado válido.");
                    }
                    break;

                case "usuarios":
                    if (argumento == "") {
                        json.Add("type", "USERS");
                        mensaje = JsonConvert.SerializeObject(json);
                        Envia(Parser.CadenaABytes(mensaje));
                        json = JsonConvert.DeserializeObject<Dictionary<String, String>>(MensajeRecibido());
                            if (json != null) {
                                List<string> nombres;
                                try {
                                    nombres = JsonConvert.DeserializeObject<List<string>>(json["usernames"]);
                                    controlador.Mensaje(json["usernames"]);
                                } catch (Newtonsoft.Json.JsonReaderException) {
                                    controlador.Error("El mensaje recibido no es válido");
                                    enchufe.Close();
                                    Environment.Exit(0);
                                }
                            } else {
                                controlador.Error("Ocurrió un error con el servidor");
                                enchufe.Close();
                                Environment.Exit(0);
                            }
                    
                    } else {
                        cuarto = argumento;
                        json.Add("type", "ROOM_USERS");
                        json.Add("roomname", cuarto);
                        mensaje = JsonConvert.SerializeObject(json);
                        Envia(Parser.CadenaABytes(mensaje));
                        json = JsonConvert.DeserializeObject<Dictionary<String, String>>(MensajeRecibido());
                        if (json != null) {
                            if (json["type"] == "ROOM_USER_LIST") {
                                
                                List<string> nombres;
                                try {
                                    nombres = JsonConvert.DeserializeObject<List<string>>(json["usernames"]);
                                    controlador.Mensaje(json["usernames"]);
                                } catch (Newtonsoft.Json.JsonReaderException) {
                                    controlador.Error("El mensaje recibido no es válido");
                                    enchufe.Close();
                                    Environment.Exit(0);
                                }
                                
                            } else if (json["type"] == "WARNING"){
                                controlador.Mensaje("Error: " + json["message"]);
                            }
                        } else {
                            controlador.Error("Ocurrió un error con el servidor");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                    
                    }

                    break;

                case "cuarto":
                    json.Add("type", "NEW_ROOM");
                    json.Add("roomname", argumento);
                    mensaje = JsonConvert.SerializeObject(json);
                    Envia(Parser.CadenaABytes(mensaje));
                    json = JsonConvert.DeserializeObject<Dictionary<String, String>>(MensajeRecibido());
                    if (json != null) {
                        if (json["type"] == "INFO") {
                            controlador.Mensaje("El cuarto se creó exitosamente");
                            return;
                        } else if (json["type"] == "WARNING"){
                            controlador.Mensaje("Error: " + json["message"]);
                        }
                    } else {
                        controlador.Error("Ocurrió un error con el servidor");
                        enchufe.Close();
                        Environment.Exit(0);
                    }
                    break;

                case "invitar":
                    String[] separado = argumento.Split(", ");
                    cuarto = separado[0];
                    List<String> usuarios = new List<string>();
                    for (int i = 1; i < separado.Length; i++) {
                        usuarios.Add(separado[i]);
                    }
                    json.Add("type", "INVITE");
                    json.Add("roomname", cuarto);
                    json.Add("usernames", JsonConvert.SerializeObject(usuarios));
                    mensaje = JsonConvert.SerializeObject(json);
                    Envia(Parser.CadenaABytes(mensaje));

                    json = JsonConvert.DeserializeObject<Dictionary<String, String>>(MensajeRecibido());
                    if (json != null) {
                        if (json["type"] == "INFO") {
                            controlador.Mensaje("La invitación se realizó exitosamente");
                            return;
                        } else if (json["type"] == "WARNING"){
                            controlador.Mensaje("Error: " + json["message"]);
                        }
                    } else {
                        controlador.Error("Ocurrió un error con el servidor");
                        enchufe.Close();
                        Environment.Exit(0);
                    }
                    
                    break;

                case "unirse":
                    cuarto = argumento;
                    json.Add("type", "JOIN_ROOM");
                    json.Add("roomname", cuarto);
                    mensaje = JsonConvert.SerializeObject(json);
                    Envia(Parser.CadenaABytes(mensaje));

                    json = JsonConvert.DeserializeObject<Dictionary<String, String>>(MensajeRecibido());
                    if (json != null) {
                        if (json["type"] == "INFO") {
                            controlador.Mensaje("Te has unido al cuarto '" + cuarto + "'");
                            return;
                        } else if (json["type"] == "WARNING"){
                            controlador.Mensaje("Error: " + json["message"]);
                        }
                    } else {
                        controlador.Error("Ocurrió un error con el servidor");
                        enchufe.Close();
                        Environment.Exit(0);
                    }

                    break;

                case "salir":
                    cuarto = argumento;
                    json.Add("type", "LEAVE_ROOM");
                    json.Add("roomname", cuarto);
                    mensaje = JsonConvert.SerializeObject(json);
                    Envia(Parser.CadenaABytes(mensaje));

                    json = JsonConvert.DeserializeObject<Dictionary<String, String>>(MensajeRecibido());
                    if (json != null) {
                        if (json["type"] == "INFO") {
                            controlador.Mensaje("Has abandonado el cuarto '" + cuarto + "'");
                            return;
                        } else if (json["type"] == "WARNING"){
                            controlador.Mensaje("Error: " + json["message"]);
                        }
                    } else {
                        controlador.Error("Ocurrió un error con el servidor");
                        enchufe.Close();
                        Environment.Exit(0);
                    }
                    break;

                case "desconectar":
                    estaActivo = false;
                    json.Add("type", "DISCONNECT");
                    mensaje = JsonConvert.SerializeObject(json);
                    Envia(Parser.CadenaABytes(mensaje));
                    Environment.Exit(0);
                    break;

                default:
                    EnviaMensajePublico("/" + comando + " " + argumento);
                    break;
            }
            
        }

        //regresa un mensaje que recibe del servidor asegurándose de que el otro hilo de ejecución no lo haya recibido ya
        private String MensajeRecibido() {
            while (estaEscuchando) {}
            lock(enchufe) {
                if (guardado == "") {
                    return Recibe();
                } else {
                    String recibido = guardado;
                    guardado = "";
                    return recibido;
                }
            }
        }
    } 
}