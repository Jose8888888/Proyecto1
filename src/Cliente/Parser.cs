using System;   
using System.Text;  

public static class Parser  
    {  
        //convierte una cadena en un arreglo de bytes para mandarlo por el enchufe
        public static byte[] CadenaABytes(String cadena) {
            byte[] bytes = Encoding.UTF8.GetBytes(cadena);
            return bytes;
        }

        //convierte un arreglo de bytes de un enchufe en una cadena
        public static String BytesACadena(byte[] bytes) {
            return Encoding.UTF8.GetString(bytes);
        }
    }