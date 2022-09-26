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

        private Socket enchufe;
        private String nombre;
        private Estado estado;
        
        public Usuario(Socket enchufe) {
            this.enchufe = enchufe;
        }

        public Socket GetSocket() {
            return enchufe;
        }

        public void SetSocket(Socket enchufe) {
            this.enchufe = enchufe;
        }

        public String GetNombre() {
            return nombre;
        }

        public void setNombre(String nombre) {
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