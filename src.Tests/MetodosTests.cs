using System;  

namespace Tests {
    public static class MetodosTests  
    {  
        public static String NombreAleatorio() {
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