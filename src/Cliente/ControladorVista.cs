using System;  
  
  namespace Chat {

    //clase que conecta al cliente y al servidor con la vista
    public  class ControladorVista  
    {  
        private Vista vista;
        private ControladorCliente controlador;

        public ControladorVista() {
          vista = new Vista();
        }
        public ControladorVista(Cliente cliente) {
          controlador = new ControladorCliente(cliente);
          vista = new Vista(controlador);
        }

        //muestra un mensaje en la vista
        public void Mensaje(String mensaje) {
            vista.Mensaje(mensaje);
        }

        //le pide el nombre de usuario a la vista
        public void PideNombre() {
          vista.PideNombre();
        }

        //muestra un mensaje de error en la vista
        public void Error(String mensaje) {
            vista.Error(mensaje);
        }

        //espera a que el usuario escriba algo en la terminal
        public String Escucha() {
          return vista.Escucha();
        }
        
    }
  }