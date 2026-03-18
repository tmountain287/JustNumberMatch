package com.yourcompany.googlelogin;

import org.json.JSONObject;
import org.json.JSONException;

import android.app.Activity;
import android.content.Intent;
import android.util.Log;

import com.google.android.gms.auth.api.signin.*;
import com.google.android.gms.common.api.ApiException;
import com.google.android.gms.tasks.Task;
import com.unity3d.player.UnityPlayer;

public class GoogleLoginPlugin {
    private static final String TAG = "GoogleLogin";
    private static final int RC_SIGN_IN = 9001;

    private static GoogleSignInClient mGoogleSignInClient;
    private static Activity activity;

    // UnityPlayerActivity에서 호출: GoogleLoginPlugin.init(this, webClientId);
    public static void init(Activity currentActivity, String webClientId) {
        activity = currentActivity;

        GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DEFAULT_SIGN_IN)
                .requestIdToken(webClientId) // 서버 검증/ Firebase Auth 연동에 필요
                .requestEmail()
                .build();

        mGoogleSignInClient = GoogleSignIn.getClient(activity, gso);
    }

    public static void signIn() {
        if (mGoogleSignInClient == null || activity == null) {
            Log.e(TAG, "signIn: GoogleSignInClient or Activity is null. Call init() first.");
            UnityPlayer.UnitySendMessage("PlatformLoginReceiver", "OnLoginFailed", "not_initialized");
            return;
        }
        Intent signInIntent = mGoogleSignInClient.getSignInIntent();
        activity.startActivityForResult(signInIntent, RC_SIGN_IN);
    }

    public static void signOut() {
        if (mGoogleSignInClient != null) {
            mGoogleSignInClient.signOut();
        }
    }

    // UnityPlayerActivity의 onActivityResult에서 위임 호출 필요
    public static void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode != RC_SIGN_IN) return;

        Task<GoogleSignInAccount> task = GoogleSignIn.getSignedInAccountFromIntent(data);
        try {
            GoogleSignInAccount account = task.getResult(ApiException.class);

            // 안전 추출 (null → JSONObject.NULL 로 직렬화)
            String idToken       = account.getIdToken();        // JWT (로그 금지)
            String email         = account.getEmail();
            String displayName   = account.getDisplayName();
            String givenName     = account.getGivenName();
            String familyName    = account.getFamilyName();
            String id            = account.getId();             // Google user id (sub)
            String serverAuthCode= account.getServerAuthCode(); // (server auth code flow일 때)
            String photoUrl      = account.getPhotoUrl() != null ? account.getPhotoUrl().toString() : null;

            try {
                JSONObject obj = new JSONObject();
                obj.put("idToken",        idToken != null ? idToken : JSONObject.NULL);
                obj.put("email",          email != null ? email : JSONObject.NULL);
                obj.put("displayName",    displayName != null ? displayName : JSONObject.NULL);
                obj.put("givenName",      givenName != null ? givenName : JSONObject.NULL);
                obj.put("familyName",     familyName != null ? familyName : JSONObject.NULL);
                obj.put("id",             id != null ? id : JSONObject.NULL);
                obj.put("serverAuthCode", serverAuthCode != null ? serverAuthCode : JSONObject.NULL);
                obj.put("photoUrl",       photoUrl != null ? photoUrl : JSONObject.NULL);

                UnityPlayer.UnitySendMessage("PlatformLoginReceiver", "OnLoginSuccess", obj.toString());
            } catch (JSONException je) {
                Log.e(TAG, "JSON build failed", je);
                UnityPlayer.UnitySendMessage("PlatformLoginReceiver", "OnLoginFailed", "json_error");
            }

        } catch (ApiException e) {
            Log.e(TAG, "SignIn failed: " + e.getStatusCode());
            UnityPlayer.UnitySendMessage("PlatformLoginReceiver", "OnLoginFailed", String.valueOf(e.getStatusCode()));
        }
    }
}
