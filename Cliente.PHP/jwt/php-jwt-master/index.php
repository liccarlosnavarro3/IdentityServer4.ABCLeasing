<!DOCTYPE html>
<!--
To change this license header, choose License Headers in Project Properties.
To change this template file, choose Tools | Templates
and open the template in the editor.
-->
<html>
    <head>
        <meta charset="UTF-8">
        <title></title>
    </head>
    <body>
        <?php
            use Firebase\JWT\JWT;
            require_once './php-jwt-master/src/JWT.php';        
            $time = time(); //Fecha y hora actual en segundos
            $key = "example_key";
            $token = array(
                'iat' => $time, // Tiempo que inició el token
                'exp' => $time + (60 * 60), // Tiempo que expirará el token (+1 hora)                                                                
                'idUsuario' =>'1',//Informacion de usuario
            );
            $jwt = JWT::encode($token, $key);//Codificamos el Token
            $decoded = JWT::decode($jwt, $key, array('HS256'));//Decodificamos el Token
            print_r($jwt);//Mostramos el Tocken Codificado
            echo '<br><br>';
            print_r($decoded);//Mostramos el Tocken Decodificado
        ?>
    </body>
</html>
