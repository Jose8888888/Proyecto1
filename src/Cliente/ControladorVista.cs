using System;  
  
  namespace Controlador {

    //clase que conecta al cliente y al servidor con la vista
    public  class ControladorVista  
    {  
        private Vista.Vista vista;
        private ControladorCliente controlador;

        public ControladorVista() {
          vista = new Vista.Vista();
        }
        public ControladorVista(Cliente.Cliente cliente) {
          controlador = new ControladorCliente(cliente);
          vista = new Vista.Vista(controlador);
        }

        //muestra un mensaje en la vista
        public void Mensaje(String mensaje) {
            vista.Mensaje(mensaje);
        }

        //le pide el nombre de usuario a la vista
        public void PideNombre() {
          vista.PideNombre();
        }
        
    }
  }