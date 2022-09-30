using System;  
using Controlador;
  
  namespace Vista {
    public class Vista  
    {  
        private Controlador.ControladorCliente controlador = new Controlador.ControladorCliente();


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
        
    }
  }