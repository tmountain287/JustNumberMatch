package com.yourcompany.googlelogin;

import android.content.Intent;
import com.unity3d.player.UnityPlayerActivity;

public class CustomUnityActivity extends UnityPlayerActivity {
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        // 여기서 Plugin 쪽으로 전달
        GoogleLoginPlugin.onActivityResult(requestCode, resultCode, data);
    }
}
