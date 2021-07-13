<?php
/*
 * $Url_Auth_Test = authfacttest.abcleasing.com.mx;
 * $Url_Api_Test = apifacttest.abcleasing.com.mx;
 */
$access_token = "token=3e85191eaf936c37334404be307c84c4";//$arr_json_data->"a724ca7ed2d7523fffd6bde2f5a0eaa1";
$curl = curl_init();
 
curl_setopt_array($curl, array(
  //CURLOPT_URL => "https://ids.abcleasing.com.mx/connect/accesstokenvalidation",
  CURLOPT_URL => "http://localhost:55503/connect/accesstokenvalidation",
//  CURLOPT_URL => "https://ids.abcleasing.com.mx/connect/userinfo",
  CURLOPT_RETURNTRANSFER => true,
  CURLOPT_CUSTOMREQUEST => "POST",
  CURLOPT_HTTPHEADER => array(
    "Authorization: Bearer {$access_token}"
  )
));
 
$curl_response = curl_exec($curl);
$curl_error = curl_error($curl);

//var_dump  ($curl);
curl_close($curl);
 
if ($curl_error) {
  echo "Error in the CURL response from DEMO API:" . $curl_error;
} else {
  echo "<br>Demo API Response:" . $curl_response;
  echo "<br>Error in the CURL response from DEMO API:" . $curl_error;
}