<?php

$authorize_url = "http://localhost:55503/connect/authorize";
$token_url = "http://localhost:55503/connect/token";

//	callback URL specified when the application was defined--must match what API says
$callback_uri = "https://localhost:55503/signin-oidc";

$test_api_url = "http://localhost:59655/api/Values";

//	client (application) credentials - located at apim.byu.edu
$client_id = "ABCLeasing";
$client_secret = "785BFB36-9A18-4D29-94A5-18FE176F2E6E";

if ($_POST["access_token"]) {
	//	what to do if there's an access token
	$resource = getResource($_POST["access_token"]);
	echo $resource;
} elseif ($_POST["hidden_token"]) {
	$resource = getResource($_POST["hidden_token"]);
	echo $resource;
} else {
	//	what to do if there's no access token
	getAccessToken();
}



//	step A - single call with client ID and callback on the URL
function getAccessToken() {
	global $authorize_url, $client_id, $callback_uri, $token_url;

	$authorization_redirect_url = $authorize_url . "?response_type=token&client_id=" . $client_id . "&redirect_uri=" . $callback_uri . "&scope=ABCLeasingAPI";

	//	create form
	echo "Go <a href='$authorization_redirect_url'>here</a>, copy the code, and paste it into the box below.<br /><form id='get_token' action=" . $_SERVER["PHP_SELF"] . " method = 'post'><input type='text' name='access_token' /><br /><input type='submit'><input type='hidden' name='hidden_token' id='hidden_token'/></form>";

	//	use JavaScript to check for access_token in URL
	//		redirects if it doesn't exist
	//		submits form if it does
	echo "<script type='text/javascript'>if (window.location.hash.length > 0) {var accessToken = window.location.hash; accessToken = accessToken.slice(accessToken.indexOf('access_token') + 13); accessToken = accessToken.slice(0, accessToken.indexOf('&')); document.getElementById('hidden_token').value = accessToken; document.getElementById('get_token').submit();} else {window.location.replace('$authorization_redirect_url');}</script>";
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