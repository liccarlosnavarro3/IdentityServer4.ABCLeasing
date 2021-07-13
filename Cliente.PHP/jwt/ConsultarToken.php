<?php
////$url="https://idsws.abcleasing.com.mx/api/Values";
$url="http://localhost:59655/api/Values";

$token ='43edf4a27ff6f88232d1f7bb323a76bb';//Aquí pongo el valor del Token generado anteriormente
$headers = array('Content-Type: application/json',"Authorization: Bearer {$token}");
$process = curl_init();
curl_setopt($process, CURLOPT_URL, $url);
curl_setopt($process, CURLOPT_HTTPHEADER, $headers);
curl_setopt($process, CURLOPT_CUSTOMREQUEST, "GET");
#curl_setopt($process, CURLOPT_HEADER, 1);
curl_setopt($process, CURLOPT_TIMEOUT, 30);
curl_setopt($process, CURLOPT_HTTPGET, 1);
#curl_setopt($process, CURLOPT_VERBOSE, 1);
curl_setopt($process, CURLOPT_SSL_VERIFYPEER, false);
curl_setopt($process, CURLOPT_RETURNTRANSFER, TRUE);
$return = curl_exec($process);

curl_close($process);
echo $return;