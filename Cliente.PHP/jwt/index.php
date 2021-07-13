<?php

/*
 * $Url_Auth_Test = authfacttest.abcleasing.com.mx;
 * $Url_Api_Test = apifacttest.abcleasing.com.mx;
 */


//$url="https://idsws.abcleasing.com.mx/api/Values";
$url="http://localhost:59655/api/Values";
$client_id = "ABCLeasingMobile";//"ABCLeasing";
$client_secret = "896C0C47-AB29-5E3A-A5B6-290F28703F7F";// "785BFB36-9A18-4D29-94A5-18FE176F2E6E";
$user_name = 'LELC830729QA1';//'ADMINPLACAS';//$_POST['user'];
$pass_name = 'ApiABC2021';//'Plac45L34se_20*20';//$_POST['pass'];
$scope = "ABCLeasingAPIMobile";// "ABCLeasingAPI";
//$tokenUrl = "https://ids.abcleasing.com.mx/connect/token";
$tokenUrl = "http://localhost:55503/connect/token";

$tokenContent = "grant_type=password&username=".$user_name."&password=".$pass_name."&scope=".$scope;
//echo '<br>'.$tokenContent.'<br>';
$authorization = base64_encode("$client_id:$client_secret");
//echo '<br>';
//echo "$authorization \n";
//echo '<br>';
$tokenHeaders = array("Authorization: Basic {$authorization}","Content-Type: application/x-www-form-urlencoded");

//$access_token = getAccessToken($tokenUrl, $client_id, $client_secret);
//echo "<br>Acces Token: ".$access_token;

$token = curl_init();
curl_setopt($token, CURLOPT_URL, $tokenUrl);
curl_setopt($token, CURLOPT_HTTPHEADER, $tokenHeaders);
curl_setopt($token, CURLOPT_SSL_VERIFYPEER, false);
curl_setopt($token, CURLOPT_RETURNTRANSFER, true);
curl_setopt($token, CURLOPT_POST, true);
curl_setopt($token, CURLOPT_POSTFIELDS, $tokenContent);
//curl_setopt($token, CURLOPT_POSTFIELDS, $access_token);
$response = curl_exec($token);
curl_close ($token);
echo '<br> Response: ';
echo $response;
echo '<br>';
$token_array = json_decode($response, true);
//echo '<br>';
//echo "<br>Token: ".$token;
//print_r($token);
echo '<br>Token Array: ';
print_r($token_array);
echo '<br>';
echo "\n now calling $url \n";
echo '<br>';
$headers = array('Content-Type: application/json',"Authorization: Bearer {$token_array["access_token"]}");
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
//echo "<br>Process: ".$process;
curl_close($process);
echo $return;

/*
Response: {"access_token":"7ad43f0153c4f571cfce911e468968e7","expires_in":3600,"token_type":"Bearer"}

Token Array: Array ( [access_token] => 7ad43f0153c4f571cfce911e468968e7 [expires_in] => 3600 [token_type] => Bearer )
now calling https://idsws.abcleasing.com.mx/api/Values
{"message":"OK User","IdClient":"1","Client":"ABCLeasing","IdUser":"1","UserName":"Admin","Email":"cnavarro@abcleasing.com.mx","Role":"Admin"}*/

function getAccessToken($token_url, $client_id, $client_secret) {
	$content = "grant_type=client_credentials";
	$authorization = base64_encode("$client_id:$client_secret");
	$header = array("Authorization: Basic {$authorization}","Content-Type: application/x-www-form-urlencoded");

	$curl = curl_init();
	curl_setopt_array($curl, array(
		CURLOPT_URL => $token_url,
		CURLOPT_HTTPHEADER => $header,
		CURLOPT_SSL_VERIFYPEER => false,
		CURLOPT_RETURNTRANSFER => true,
		CURLOPT_POST => true,
		CURLOPT_POSTFIELDS => $content
	));
	$response = curl_exec($curl);
        echo "<br>Curl: ".$curl;
	curl_close($curl);
        echo "<br>Response2: ".$response;
        
	return json_decode($response)->access_token;
}