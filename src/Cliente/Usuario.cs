using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  

namespace Chat {

    

    //clase para los usuarios que se guardan en el servidor
    public class Usuario  {  

        //los estados en los que puede estar un usuario
        public enum Estado {
        NINGUNO,
        ACTIVE,
        AWAY,
        BUSY
        }

        #pragma warning disable CS8625
        private String nombre = null;
        private Estado estado;
        byte[] bytes = new byte[2048]; 
        private List<Cuarto> cuartos = new List<Cuarto>();
        private List<Cuarto> invitaciones = new List<Cuarto>();        




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

        public List<Cuarto> GetCuartos() {
            return cuartos;
        }

        public void AgregaCuarto(Cuarto cuarto) {
            cuartos.Add(cuarto);
        }

        public void AgregaInvitacion(Cuarto invitacion) {
            invitaciones.Add(invitacion);
        }

        public void EliminaCuarto(Cuarto cuarto) {
            cuartos.Remove(cuarto);
        }

        //regresa true si el usuario está en el cuarto que recibe
        public bool EstaEnCuarto(Cuarto cuarto) {
            foreach (Cuarto c in cuartos) {
                if (c == cuarto) {
                    return true;
                }
            }
            return false;
        }

        //regresa true si el usuario está en invitado al cuarto que recibe
        public bool EstaInvitado(Cuarto cuarto) {
            foreach (Cuarto c in invitaciones) {
                if (c == cuarto) {
                    return true;
                }
            }
            return false;
        }

    }
}
