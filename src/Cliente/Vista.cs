using System;  
  
  namespace Chat {
    public class Vista  
    {  
      private ControladorCliente controlador;


      public Vista() {}
      public Vista(ControladorCliente controlador) {
        this.controlador = controlador;
      }

        //Te pide tu nombre y se lo manda al controlador
      public void PideNombre() {
          Console.WriteLine("Escribe tu nombre de usuario: ");
          String linea = Console.ReadLine();
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
            return Console.ReadLine();
        }

        //pide la IP
        public String PideIP() {
          Console.WriteLine("Escribe la IP: ");
          return Console.ReadLine();
        }

        //pide el puerto
        public int PidePuerto() {
          Console.WriteLine("Escribe el puerto: ");
          return int.Parse(Console.ReadLine());        }
    }
  }