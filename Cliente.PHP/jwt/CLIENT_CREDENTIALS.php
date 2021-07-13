<?php

$token_url = "http://localhost:55503/connect/token";

$test_api_url = "http://localhost:59655/api/Values";
//$test_api_url = "http://localhost:5001/weatherforecast";

//	client (application) credentials on apim.byu.edu
//$client_id = "mvc";
//$client_secret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0";
$client_id = "ABCLeasing";
$client_secret = "785BFB36-9A18-4D29-94A5-18FE176F2E6E";

$access_token = getAccessToken();
$resource = getResource($access_token);
var_dump($access_token);
echo '<br>';
var_dump($resource);



//	step A, B - single call with client credentials as the basic auth header
//		will return access_token
function getAccessToken() {
	global $token_url, $client_id, $client_secret;

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
	curl_close($curl);

	return json_decode($response)->access_token;
}

//	step B - with the returned access_token we can make as many calls as we want
function getResource($access_token) {
	global $test_api_url;

	$header = array("Authorization: Bearer {$access_token}");

	$curl = curl_init();
	curl_setopt_array($curl, array(
		CURLOPT_URL => $test_api_url,
		CURLOPT_HTTPHEADER => $header,
		CURLOPT_SSL_VERIFYPEER => false,
		CURLOPT_RETURNTRANSFER => true
	));
	$response = curl_exec($curl);
	curl_close($curl);

	return json_decode($response, true);
}