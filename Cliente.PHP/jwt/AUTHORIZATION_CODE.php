<?php

$authorize_url = "http://localhost:55503/connect/authorize";
$token_url = "http://localhost:55503/connect/token";

//	callback URL specified when the application was defined--has to match what the application says
$callback_uri = "https://localhost:55503/signin-oidc";

$test_api_url = "http://localhost:59655/api/Values";

//	client (application) credentials - located at apim.byu.edu
$client_id = "ABCLeasing";
$client_secret = "785BFB36-9A18-4D29-94A5-18FE176F2E6E";



if ($_POST["authorization_code"]) {
	//	what to do if there's an authorization code
	$access_token = getAccessToken($_POST["authorization_code"]);
	$resource = getResource($access_token);
	echo $resource;
} elseif ($_GET["code"]) {
	$access_token = getAccessToken($_GET["code"]);
	$resource = getResource($access_token);
	echo $resource;
} else {
	//	what to do if there's no authorization code
	getAuthorizationCode();
}



//	step A - simulate a request from a browser on the authorize_url
//		will return an authorization code after the user is prompted for credentials
function getAuthorizationCode() {
	global $authorize_url, $client_id, $callback_uri;

	$authorization_redirect_url = $authorize_url . "?response_type=code&client_id=" . $client_id . "&redirect_uri=" . $callback_uri . "&scope=ABCLeasingAPI";

	header("Location: " . $authorization_redirect_url);

	//	if you don't want to redirect
	// echo "Go <a href='$authorization_redirect_url'>here</a>, copy the code, and paste it into the box below.<br /><form action=" . $_SERVER["PHP_SELF"] . " method = 'post'><input type='text' name='authorization_code' /><br /><input type='submit'></form>";
}

//	step I, J - turn the authorization code into an access token, etc.
function getAccessToken($authorization_code) {
	global $token_url, $client_id, $client_secret, $callback_uri;

	$authorization = base64_encode("$client_id:$client_secret");
	$header = array("Authorization: Basic {$authorization}","Content-Type: application/x-www-form-urlencoded");
	$content = "grant_type=authorization_code&code=$authorization_code&redirect_uri=$callback_uri";

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

	if ($response === false) {
		echo "Failed";
		echo curl_error($curl);
		echo "Failed";
	} elseif (json_decode($response)->error) {
		echo "Error:<br />";
		echo $authorization_code;
		echo $response;
	}

	return json_decode($response)->access_token;
}

//	we can now use the access_token as much as we want to access protected resources
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