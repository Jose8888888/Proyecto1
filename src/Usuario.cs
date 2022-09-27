using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  

namespace Servidor {

    

    //clase para los usuarios que se guardan en el servidor
    public class Usuario  {  

        //los estados en los que puede estar un usuario
        public enum Estado {
        ACTIVE,
        AWAY,
        BUSY
        }

        private String nombre = "";
        private Estado estado;
        byte[] bytes = new byte[1024];  
        




        public String GetNombre() {
            return nombre;
        }

        public void SetNombre(String nombre) {
            this.nombre = nombre;
        }

        public Estado GetEstado() {
            return estado;
        }

        public void SetEstado(Estado estado) {
            this.estado = estado;
        }


    }
}