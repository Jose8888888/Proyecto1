Proyecto 1

El sistema de construcción que utilicé es cake.

Para compilar el programa y correr las pruebas unitarias use el siguiente comando:

		$ dotnet cake
		
Para correr el servidor use:

		$ dotnet run --project src/Servidor/
		
Para correr el cliente use:

		$ dotnet run --project src/Cliente/su
		


En el cliente una vez que te identifiques puedes realizar las siguientes acciones de la forma en que se indica:

-Mensaje público: Solo escriba el mensaje.	
	
-Mensaje privado: Escriba el usuario al que le quiere enviar el mensaje, seguido de dos puntos y un espacio, y después el mensaje.
	
		username: message
			
-Mensaje de un cuarto: Escriba entre corchetes el cuarto seguido de un espacio y después el mensaje.
	
		[roomname] message
			
-Cambiar de estado: Escriba "/estado " y después el estado al que quieres cambiar, que puede ser 	ACTIVE, AWAY o BUSY.
	
		/estado AWAY
			
-Ver usuarios del chat: Escriba "/usuarios"
	
		/usuarios
			
-Crear un cuarto: Escriba "/cuarto " y después el nombre del cuarto.
	
		/cuarto roomname
			
-Invitar usuarios a un cuarto: Escriba "/invitar ", después el nombre del cuarto seguido de una coma 	y un espacio, y luego los usuarios a los que quieres invitar separados por una coma y un espacio.
	
		/invitar roomname, username1, username2, username3
			
-Unirse a un cuarto: Escriba "/unirse " y después el nombre del cuarto.
	
		/unirse roomname
			
-Ver usuarios de un cuarto: Escriba "/usuarios " y después el nombre del cuarto.
	
		/usuarios roomname
			
-Salir de un cuarto: Escriba "/salir " y después el nombre del cuarto.
	
		/salir roomname
			
-Desconectarse del chat: Escriba "/desconectar".
	
		/desconectar
			
			
Los mensajes que recibas se verán de la siguiente forma:

-Mensaje público: El usuario que envía el mensaje seguido de dos puntos y un espacio, y después el 	mensaje.
	
		username: message
		
-Mensaje privado: El usuario que envía el mensaje entre paréntesis, seguido de dos puntos y un 		espacio, y después el mensaje.
	
		(username): message
		
-Mensaje de un cuarto: El nombre del cuarto entre corchetes seguido de un espacio, después el 		usuario que envía el mensaje seguido de dos puntos y un espacio, y luego el mensaje.
	
		[roomname] username: message
		
		

Las clases que conforman los programas son las siguientes:
	
-Servidor: Recibe y maneja a los clientes.
	
-Cliente: Se conecta con el servidor.
	
-Usuario: Los usuarios que guarda el servidor cuando se conecta un cliente. Un usuario tiene nombre y estado. También tiene una lista de los cuartos en los que está y otra de los cuartos a los que está invitado.

-Cuarto: Los cuartos a los que se pueden unir los usuarios. Un cuarto tiene nombre y una lista con los usuarios que están en él.

-Vista: Conecta al controlador con la consola, recibiendo mensajes de uno y mandándoselos al otro.

-ControladorVista: Conecta al cliente y al servidor con la vista, recibiendo mensajes de los primeros y mandándoselos a la vista.

-ControladorCliente: Conecta a la vista con el cliente, recibiendo mensajes de la vista y mandándoselos al cliente.

-Parser: Contiene métodos para convertir de cadena a bytes y viceversa.
	
