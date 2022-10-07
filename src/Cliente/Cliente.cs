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
        private static IPHostEntry host = Dns.GetHostEntry("localhost");  
        private static IPAddress ipAddress = host.AddressList[0];  
        private IPEndPoint remoteEP = new IPEndPoint(ipAddress, 1234);  
  
        private static Socket enchufe = new Socket(ipAddress.AddressFamily,  
                    SocketType.Stream, ProtocolType.Tcp);  
        private ControladorVista controlador;
        private static String guardado = "";
        private static bool puedeEscuchar = true;
        private static bool estaEscuchando = false;


        public static void Main()  
        {  
            Cliente cliente = new Cliente();
            cliente.Inicia();  
            Thread hilo = new Thread(cliente.Escucha);
            hilo.Start();
            cliente.controlador = new ControladorVista(cliente);
            cliente.controlador.PideNombre();

            while(true) {
                cliente.AnalizaMensaje(cliente.controlador.Escucha());
                puedeEscuchar = true;
            }
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
            
            Dictionary<String, String> json2 = JsonConvert.DeserializeObject<Dictionary<String, String>>(MensajeRecibido());
            if (json2 != null) {
                if (json2["type"] == "INFO") {
                    controlador.Mensaje("Nombre aceptado");
                    return;
                } else if (json2["type"] == "WARNING"){
                    controlador.Mensaje("Error: " + json2["message"]);
                    controlador.PideNombre();
                }
            } else {
                controlador.Error("Ocurrió un error con el servidor");
                enchufe.Close();
                Environment.Exit(0);
            }
        }

        //revisa si es un comando o un mensaje es público, privado, etc.
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
                String[] separados = mensaje.Split(": ", 2);
                if (separados.Length == 2) {
                EnviaMensaje(separados[0], separados[1]);
                return;
                } else {
                    EnviaMensaje(mensaje);
                }
            }
                
            
        }

        //envía un mensaje privado
        private void EnviaMensaje(String destinatario, String mensaje) {
            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "MESSAGE");
            json.Add("username", destinatario);
            json.Add("message", mensaje);
            String mensajeJson = JsonConvert.SerializeObject(json);
            Envia(Parser.CadenaABytes(mensajeJson));

        }

        //envía un mensaje público
        private void EnviaMensaje(String mensaje) {
            Dictionary<string, string> json = new Dictionary<string, string>();
            json.Add("type", "PUBLIC_MESSAGE");
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
                    } else {
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
                        
                    }
        }

        //analiza un comando que recibe de la terminal
        private void AnalizaComando(String comando, String argumento) {
            Dictionary<string, string> json;
            switch(comando) {
                case "estado":
                    if (argumento == "ACTIVE" || argumento == "AWAY" || argumento == "BUSY") {
                        json = new Dictionary<string, string>();
                        json.Add("type", "STATUS");
                        json.Add("status", argumento);
                        String msj = JsonConvert.SerializeObject(json);
                        Envia(Parser.CadenaABytes(msj));

                        json = JsonConvert.DeserializeObject<Dictionary<String, String>>(MensajeRecibido());
                        if (json != null) {
                            if (json["type"] == "INFO") {
                                controlador.Mensaje("El estado cambió exitosamente");
                                return;
                            } else if (json["type"] == "WARNING"){
                                controlador.Mensaje("WARNING: " + json["message"]);
                            }
                        } else {
                            controlador.Error("Ocurrió un error con el servidor");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                    } else {
                        controlador.Mensaje(argumento + " no es un estado válido.");
                    }
                    break;

                case "usuarios":
                    json = new Dictionary<string, string>();
                    json.Add("type", "USERS");
                    String mensaje = JsonConvert.SerializeObject(json);
                    Envia(Parser.CadenaABytes(mensaje));
                    json = JsonConvert.DeserializeObject<Dictionary<String, String>>(MensajeRecibido());
                        if (json != null) {
                            controlador.Mensaje(json["usernames"]);
                        } else {
                            controlador.Error("Ocurrió un error con el servidor");
                            enchufe.Close();
                            Environment.Exit(0);
                        }
                    
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