using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class FirebaseAuth
{
    const string FirebaseAPIkey = "AIzaSyDysXaUC9yev74AxQ9hooZ2abjCv-6fYVE";
    const string SignUpURL = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key=" + FirebaseAPIkey;
    const string SignInURL = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key=" + FirebaseAPIkey;
    const string RefreshURL = "https://securetoken.googleapis.com/v1/token?key=" + FirebaseAPIkey;
    const string ConfirmationURL = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/getOobConfirmationCode?key=" + FirebaseAPIkey;
    const string SetAccountURL = "https://www.googleapis.com/identitytoolkit/v3/relyingparty/setAccountInfo?key=" + FirebaseAPIkey;

    public static IEnumerator SignUpWithEmailAndPassword(string email, string password, Action<string, string> onSignUp)
    {
        var request = new SignUpWithEmailAndPasswordRequest
        {
            email = email,
            password = password,
        };

        string payload = JsonConvert.SerializeObject(request);

        using (var www = PostRequest(SignUpURL, payload))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(www.downloadHandler.text);
                Debug.LogErrorFormat("Error: {0} Code: {1}", errorResponse.error.message, errorResponse.error.code);
            }
            else
            {
                var signInResponse = JsonConvert.DeserializeObject<SignUpWithEmailAndPasswordResponse>(www.downloadHandler.text);
                onSignUp(signInResponse.refreshToken, signInResponse.idToken);
            }
        }
    }

    public static IEnumerator SignInWithEmailAndPassword(string email, string password, Action<string, string> onSignIn)
    {
        var request = new SignInWithEmailAndPasswordRequest
        {
            email = email,
            password = password,
        };

        string payload = JsonConvert.SerializeObject(request);

        using (var www = PostRequest(SignInURL, payload))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(www.downloadHandler.text);
                Debug.LogFormat("Error: {0} Code: {1}", errorResponse.error.message, errorResponse.error.code);
            }
            else
            {
                var signInResponse = JsonConvert.DeserializeObject<SignInWithEmailAndPasswordResponse>(www.downloadHandler.text);
                onSignIn(signInResponse.refreshToken, signInResponse.idToken);
            }
        }
    }
    
    public static IEnumerator SignInWithRefreshToken(string refreshToken, Action<string, string> onSignIn)
    {
        Dictionary<string, string> payload = new Dictionary<string, string>()
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
        };

        using (var www = UnityWebRequest.Post(RefreshURL, payload))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(www.downloadHandler.text);
                Debug.LogFormat("Error: {0} Code: {1}", errorResponse.error.message, errorResponse.error.code);
            }
            else
            {
                var signInResponse = JsonConvert.DeserializeObject<SignInWithRefreshTokenResponse>(www.downloadHandler.text);
                onSignIn(signInResponse.refresh_token, signInResponse.id_token);
            } 
        }
    }

    public static IEnumerator SignInAnonymously(Action<string, string> onSignIn)
    {
        var payload = JsonConvert.SerializeObject(new SignInAnonymouslyRequest());

        using (var www = PostRequest(SignUpURL, payload))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(www.downloadHandler.text);
                Debug.LogFormat("Error: {0} Code: {1}", errorResponse.error.message, errorResponse.error.code);
            }
            else
            {
                var signInResponse = JsonConvert.DeserializeObject<SignInAnonymouslyResponse>(www.downloadHandler.text);
                onSignIn(signInResponse.refreshToken, signInResponse.idToken);
            }
        }
    }

    public static IEnumerator SendEmailVerification(string idToken, Action<string> OnEmailVerificationSent)
    {
        var request = new SendEmailVerificationRequest
        {
            idToken = idToken,
        };

        var payload = JsonConvert.SerializeObject(request);

        using (var www = PostRequest(ConfirmationURL, payload))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                var response = JsonConvert.DeserializeObject<ErrorResponse>(www.downloadHandler.text);
                Debug.LogErrorFormat("Error: {0} Code: {1}", response.error.message, response.error.code);
            }
            else
            {
                var response = JsonConvert.DeserializeObject<SendEmailVerificationResponse>(www.downloadHandler.text);
                OnEmailVerificationSent(response.email);
            }
        }
    }

    public static IEnumerator ConfirmEmailVerification(string verificationCode, Action<bool, string> OnEmailVerificationConfirmed)
    {
        var request = new ConfirmEmailVerificationRequest
        {
            oobCode = verificationCode,
        };

        var payload = JsonConvert.SerializeObject(request);

        using (var www = PostRequest(ConfirmationURL, payload))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                var response = JsonConvert.DeserializeObject<ErrorResponse>(www.downloadHandler.text);
                Debug.LogErrorFormat("Error: {0} Code: {1}", response.error.message, response.error.code);
            }
            else
            {
                var response = JsonConvert.DeserializeObject<ConfirmEmailVerificationResponse>(www.downloadHandler.text);
                OnEmailVerificationConfirmed(response.emailVerified, response.email);
            }
        }
    }

    static UnityWebRequest PostRequest(string URL, string payload)
    {
        var www = new UnityWebRequest
        {
            url = URL,
            method = UnityWebRequest.kHttpVerbPOST
        };
        
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(payload);

        www.uploadHandler = new UploadHandlerRaw(bodyRaw)
        {
            contentType = "application/json"
        };

        www.downloadHandler = new DownloadHandlerBuffer();

        return www;
    }
}

#pragma warning disable IDE1006 // Naming Styles

class Error2
{

    public string domain { get; set; }

    public string reason { get; set; }
    public string message { get; set; }
}

class Error
{
    public List<Error2> errors { get; set; }
    public int code { get; set; }
    public string message { get; set; }
}

class ErrorResponse
{
    public Error error { get; set; }
}

class SignInWithEmailAndPasswordRequest
{
    public string email { get; set; }
    public string password { get; set; }
    bool returnSecureToken = true;
}

class SignInWithEmailAndPasswordResponse
{
    public string kind { get; set; }
    public string localId { get; set; }
    public string email { get; set; }
    public string displayName { get; set; }
    public string idToken { get; set; }
    public bool registered { get; set; }
    public string refreshToken { get; set; }
    public string expiresIn { get; set; }
}

class SignInWithRefreshTokenResponse
{
    public string expires_in { get; set; }
    public string token_type { get; set; }
    public string refresh_token { get; set; }
    public string id_token { get; set; }
    public string user_id { get; set; }
    public string project_id { get; set; }
}

class SignUpWithEmailAndPasswordRequest
{
    public string email { get; set; }
    public string password { get; set; }
    public bool returnSecureToken { get; set; }
}

class SignUpWithEmailAndPasswordResponse
{
    public string kind { get; set; }
    public string idToken { get; set; }
    public string email { get; set; }
    public string refreshToken { get; set; }
    public string expiresIn { get; set; }
    public string localId { get; set; }
}

class SignInAnonymouslyRequest
{
    bool returnSecureToken = true;
}

class SignInAnonymouslyResponse
{
    public string kind { get; set; }
    public string idToken { get; set; }
    public string email { get; set; }
    public string refreshToken { get; set; }
    public string expiresIn { get; set; }
    public string localId { get; set; }
}

class SendEmailVerificationRequest
{
    string requestType = "VERIFY_EMAIL";
    public string idToken { get; set; }
}

class SendEmailVerificationResponse
{
    public string kind { get; set; }
    public string email { get; set; }
}

class ConfirmEmailVerificationRequest
{
    public string oobCode { get; set; }
}

class ProviderUserInfo
{
    public string providerId { get; set; }
    public string federatedId { get; set; }
}

class ConfirmEmailVerificationResponse
{
    public string kind { get; set; }
    public string email { get; set; }
    public string displayName { get; set; }
    public string photoUrl { get; set; }
    public string passwordHash { get; set; }
    public List<ProviderUserInfo> providerUserInfo { get; set; }
    public bool emailVerified { get; set; }
}

#pragma warning restore IDE1006 // Naming Styles
