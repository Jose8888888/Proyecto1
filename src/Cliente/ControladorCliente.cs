
using System;  
  
  namespace Chat {

    //clase que conecta a la vista con el cliente
    public class ControladorCliente
    {  
        private Cliente cliente;


        public ControladorCliente(Cliente cliente) {
          this.cliente = cliente;
        }

        //recibe el nombre del usuario y se lo manda al modelo
        public void RecibeNombre(String nombre) {
            cliente.Identifica(nombre);
        }
    }
  }