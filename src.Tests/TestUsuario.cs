using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Linq;
using NUnit.Framework;
using Servidor;

namespace Tests {
    
    [TestFixture]
    public class TestUsuario  
    {  

        private static IPHostEntry host = Dns.GetHostEntry("localhost");  
        private static IPAddress ipAddress = host.AddressList[0];  
        private IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);    
        private static Socket enchufe = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Usuario usuario = new Usuario(enchufe);


        [Test]
        public void TestGetEnchufe() {
        Assert.IsTrue(usuario.GetSocket().Equals(enchufe));
        }

        [Test]
        public void TestGetNombre() {
            var caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var chars = new char[new Random().Next(20)];
            var random = new Random();

            for (int i = 0; i < chars.Length; i++) {
                chars[i] = caracteres[random.Next(caracteres.Length)];
            }

            String nombreAleatorio = new String(chars);
            usuario.setNombre(nombreAleatorio);
            Assert.IsTrue(usuario.GetNombre().Equals(nombreAleatorio));
        }

        [Test]
        public void TestGetEstado() {
                foreach (int i in Enum.GetValues(typeof(Usuario.Estado))) {
                Usuario.Estado estado = (Usuario.Estado)i;
                usuario.SetEstado(estado);
                Assert.IsTrue(usuario.GetEstado() == (estado));

            }        }

        
        
    }
}
