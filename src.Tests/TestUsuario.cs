using System;  
using NUnit.Framework;
using Servidor;

namespace Tests {
    
    
    [TestFixture]
    public class TestUsuario  
    {  

        Usuario usuario = new Usuario();
        

        [Test]
        public void TestGetNombre() {
            String nombreAleatorio = MetodosTests.NombreAleatorio();
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

        
        
    }
}
