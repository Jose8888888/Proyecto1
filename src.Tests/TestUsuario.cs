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
        [Test]
        public void TestGetters() {
            IPHostEntry host = Dns.GetHostEntry("localhost");  
            IPAddress ipAddress = host.AddressList[0];  
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);    
            Socket enchufe = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Usuario usuario = new Usuario(enchufe);
            Assert.IsTrue(usuario.GetSocket().Equals(enchufe));

            var caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var chars = new char[new Random().Next(20)];
            var random = new Random();

            for (int i = 0; i < chars.Length; i++) {
                chars[i] = caracteres[random.Next(caracteres.Length)];
            }

            String nombreAleatorio = new String(chars);
            usuario.setNombre(nombreAleatorio);
            Assert.IsTrue(usuario.GetNombre().Equals(nombreAleatorio));

            foreach (int i in Enum.GetValues(typeof(Usuario.Estado))) {
                Usuario.Estado estado = (Usuario.Estado)i;
                usuario.SetEstado(estado);
                Assert.IsTrue(usuario.GetEstado() == (estado));

            }

        }

        
    }
}
