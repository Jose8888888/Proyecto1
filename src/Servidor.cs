using System;
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Collections.Generic;

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

  	        
        public static void Main()
        {   
            Servidor servidor = new Servidor();
        
            servidor.Inicia();    

            while(true) {  
                servidor.Escucha();
            }
        }

        public void Inicia()  
        {  
           
      
            try {   
  
                escucha = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  
                escucha.Bind(localEndPoint);  
                escucha.Listen(10);  
  
                Console.WriteLine("Esperando conexión...");  
                cliente = escucha.Accept();  
                usuarios.Add(usuario);
                Console.WriteLine("Cliente recibido");  

                
            }  
            catch (Exception e)  
            {  
                Console.WriteLine(e.ToString());  
            }  
  
        }   
        
        //escucha al cliente y hace lo que le pide
        public void Escucha() {
            
            byte[] bytes;
            bytes = new byte[2048];  
            int bytesRec = cliente.Receive(bytes);  
            Console.WriteLine(bytesRec);
            String msj = Encoding.ASCII.GetString(bytes, 0, bytesRec);
            String[] palabras = msj.Split();
            String msj1 = palabras[0];
            String msj2 = "";
            for(int i = 1; i < palabras.Length; i++) {
                msj2 += palabras[i];
                if(i < palabras.Length-1) {
                    msj2 += " ";
                }
            }


            switch(msj1) {
                case "IDENTIFY": 
                    String mensaje = IdentificaUsuario(msj2);
                    cliente.Receive(CadenaABytes(mensaje));                   
                    break;

            }
        }       

        //convierte una cadena en un arreglo de bytes para mandarlo por el enchufe
        private byte[] CadenaABytes(String cadena) {
            return Encoding.ASCII.GetBytes(cadena);
        }

        //le pone nombre a un usuario si es válido, si no regresa el mensaje de error
        private String IdentificaUsuario(String nombre) {
            if(!usuario.GetNombre().Equals("")) {
                return "WARNING El usuario ya está identificado";
            } else {
                foreach(Usuario usuario in usuarios) {
                    if(nombre.Equals(usuario.GetNombre())) {
                        return "WARNING El identificador ya está siendo utilizado por otro usuario";
                    }
                }
                usuario.SetNombre(nombre);
                return "INFO success";
            }
        }
    }
}
