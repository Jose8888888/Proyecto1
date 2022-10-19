using System;  
  
  namespace Chat {
    public class Vista  
    {  

      
      #pragma warning disable CS8600
      #pragma warning disable CS8618
      private ControladorCliente controlador;


      public Vista() {}
      public Vista(ControladorCliente controlador) {
        this.controlador = controlador;
      }

        //Te pide tu nombre y se lo manda al controlador
      public void PideNombre() {
          Console.WriteLine("Escribe tu nombre de usuario: ");
          String linea = Console.ReadLine();
          if (linea != null)
            controlador.RecibeNombre(linea);
      }

        //muestra un mensaje en la terminal
      public void Mensaje(String mensaje) {
        Console.WriteLine(mensaje);
      }

         //muestra un mensaje de error en la terminal
      public void Error(String mensaje) {
        Console.Error.WriteLine(mensaje);
      }
        
        //espera a que el usuario escriba algo en la terminal
        public String Escucha() {
          String cadena = Console.ReadLine();
          if (cadena != null)
            return cadena;
          return "";

        }

        //pide la IP
        public String PideIP() {
          Console.WriteLine("Escribe la IP: ");
          String IP = Console.ReadLine();
          if (IP != null)
            return IP;
          return "";
        }

        //pide el puerto
        public int PidePuerto() {
          Console.WriteLine("Escribe el puerto: ");
          String puerto = Console.ReadLine();
          if (puerto != null)
            return int.Parse(puerto);        
          return 0;
        }
    }
  }