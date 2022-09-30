using System;  
  
  namespace Controlador {

    //clase que conecta al cliente con la vista
    public  class ControladorVista  
    {  
        private Vista.Vista vista = new Vista.Vista();



        //muestra un mensaje en la vista
        public void Mensaje(String mensaje) {
            vista.Mensaje(mensaje);
        }
        
    }
  }