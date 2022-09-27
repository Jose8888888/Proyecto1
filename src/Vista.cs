using System;  
using Controlador;
  
  namespace Vista {
    public class Vista  
    {  
        private Controlador.Controlador controlador = new Controlador.Controlador();


        //Te pide tu nombre y se lo manda al controlador
        public void PideNombre() {
            Console.WriteLine("Escribe tu nombre de usuario: ");
            String linea = Console.ReadLine();
            controlador.RecibeNombre(linea);
        }


        //lee una línea de la terminal y se la manda al controlador
        public void LeeLinea() {
            String linea = Console.ReadLine();
            controlador.Controla(linea);
        }
        
    }
  }