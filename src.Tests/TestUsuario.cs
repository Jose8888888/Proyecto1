using System;  
using NUnit.Framework;

namespace Chat {
    
    
    [TestFixture]
    public class TestUsuario  
    {  

        Usuario usuario = new Usuario();
        

        [Test]
        public void TestGetNombre() {
            String nombreAleatorio = NombreAleatorio();
            usuario.SetNombre(nombreAleatorio);
            Assert.IsTrue(usuario.GetNombre().Equals(nombreAleatorio));
        }

        

        [Test]
        public void TestGetEstado() {
                foreach (int i in Enum.GetValues(typeof(Usuario.Estado))) {
                Usuario.Estado estado = (Usuario.Estado)i;
                usuario.SetEstado(estado);
                Assert.IsTrue(usuario.GetEstado() == (estado));

            }       
        }
        
        private static String NombreAleatorio() {
            var caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var chars = new char[new Random().Next(20)];
            var random = new Random();

            for (int i = 0; i < chars.Length; i++) {
                chars[i] = caracteres[random.Next(caracteres.Length)];
            }

            return new String(chars);
        }

        
        
    }
}
