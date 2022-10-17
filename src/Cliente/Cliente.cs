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
  
                    controlador.Mensaje("Enchufe conectado a " +  
                    enchufe.RemoteEndPoint.ToString());  
  
                
  
                }  
                catch (ArgumentNullException ane)  
                {  
                    controlador.Error("ArgumentNullException : " + ane.ToString());  
                    enchufe.Close();
                    Environment.Exit(0);
                }  
                catch (SocketException se)  
                {  
                    controlador.Error("SocketException : " + se.ToString()); 
                    enchufe.Close();
                    Environment.Exit(0); 
                }  
                catch (Exception e)  
                {  
                    controlador.Error("Unexpected exception : " + e.ToString()); 
                    enchufe.Close();
                    Environment.Exit(0);
                }  
  
            }  
            catch (Exception e)  
            {  
                controlador.Error("Ocurrió un error: " + e.ToString());  
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
            Envia(Parser.CadenaABytes(mensaje));
            
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

        //escucha los mensajes del servidor
        private void Escucha() {

            while(true) {
                if (puedeEscuchar) {
                    estaEscuchando = true;
                    Dictionary<String, String> json;
                    guardado = Recibe();
                    json = JsonConvert.DeserializeObject<Dictionary<String, String>>(guardado);

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
            switch(json["type"]) {
                        case "MESSAGE_FROM": 
                            String mensaje = "(" + json["username"] + "): " + json["message"];
                            controlador.Mensaje(mensaje);
                            break;
                        case "NEW_USER":
                            controlador.Mensaje(json["username"] + " ha entrado al chat.");
                            break;
                        case "NEW_STATUS":
                            controlador.Mensaje(json["username"] + " cambió su estado a " + json["status"]);
                            break;
                        case "PUBLIC_MESSAGE_FROM": 
                            mensaje = json["username"] + ": " + json["message"];
                            controlador.Mensaje(mensaje);
                            break;
                        case "INVITATION":
                            controlador.Mensaje(json["message"]);
                            break;
                        case "ROOM_MESSAGE_FROM":
                            mensaje = "[" + json["roomname"] + "] " + json["username"] + ": " + json["message"];
                            controlador.Mensaje(mensaje);
                            break;
                        case "WARNING":
                            if (json.ContainsKey("operation") && (json["operation"] == "ROOM_MESSAGE" || json["operation"] == "MESSAGE")) {
                                controlador.Error(json["message"]);
                            }
                            break;
                        case "LEFT_ROOM":
                            mensaje = json["username"] + " ha abandonado el cuarto '" + json["roomname"] + "'";
                            controlador.Mensaje(mensaje);
                            break;
                        case "DISCONNECTED":
                            controlador.Mensaje(json["username"] + " se desconectó del chat");
                        break;
                        case "ERROR":
                            controlador.Error(json["message"]);
                            enchufe.Close();
                            Environment.Exit(0);
                            break;
                        case "JOINED_ROOM":
                            controlador.Mensaje(json["username"] + " se unió al cuarto '" + json["roomname"] + "'");
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
                                controlador.Mensaje(json["usernames"]);
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
                                controlador.Mensaje(json["usernames"]);
                                
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