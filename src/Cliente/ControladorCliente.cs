
using System;  
using Cliente;
  
  namespace Controlador {

    //clase que conecta a la vista con el cliente
    public class ControladorCliente
    {  
        private Cliente.Cliente cliente;


        public ControladorCliente(Cliente.Cliente cliente) {
          this.cliente = cliente;
        }

        //recibe el nombre del usuario y se lo manda al modelo
        public void RecibeNombre(String nombre) {
            cliente.Identifica(nombre);
        }
    }
  }