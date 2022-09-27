using System;  
using Cliente;
  
  namespace Controlador {
    public  class Controlador  
    {  
        private Cliente.Cliente cliente = new Cliente.Cliente();


        //recibe el nombre del usuario y se lo manda al modelo
        public void RecibeNombre(String nombre) {
            cliente.Identifica(nombre);
        }


        //recibe un mensaje y le dice al cliente qué debe hacer dependiendo del mensaje
        public void Controla(String msj) {
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
                    cliente.Identifica(msj2);                 
                    break;

            }
        }
        
    }
  }