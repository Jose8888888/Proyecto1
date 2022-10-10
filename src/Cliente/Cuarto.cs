using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  

namespace Chat {  

    public class Cuarto  {  

        private String nombre = "";        
        private List<Usuario> miembros = new List<Usuario>();


        public Cuarto (String nombre, Usuario miembro) {
            this.nombre = nombre;
            miembros.Add(miembro);
            miembro.AgregaCuarto(this);
        }

        public String GetNombre() {
            return nombre;
        }

        public void SetNombre(String nombre) {
            this.nombre = nombre;
        }

        public List<Usuario> GetMiembros() {
            return miembros;
        }

        public void AgregaMiembro(Usuario miembro) {
            miembros.Add(miembro);
        }

        public void EliminaMiembro(Usuario miembro) {
            miembros.Remove(miembro);
        }


    }
}