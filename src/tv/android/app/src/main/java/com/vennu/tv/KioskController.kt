package com.vennu.tv

import android.app.Activity
import android.app.ActivityManager
import android.app.admin.DevicePolicyManager
import android.content.Context
import android.os.Build
import android.view.View
import android.view.WindowInsets
import android.view.WindowInsetsController
import android.view.WindowManager

class KioskController(private val activity: Activity) {
    fun activate(): KioskMode {
        activity.window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
        hideSystemUi()

        val activityManager = activity.getSystemService(ActivityManager::class.java)
        if (activityManager.lockTaskModeState != ActivityManager.LOCK_TASK_MODE_NONE) {
            return KioskMode.ALREADY_LOCKED
        }

        val policy = activity.getSystemService(DevicePolicyManager::class.java)
        return try {
            activity.startLockTask()
            if (policy.isLockTaskPermitted(activity.packageName)) {
                KioskMode.DEVICE_OWNER
            } else {
                KioskMode.PINNING_REQUESTED
            }
        } catch (_: SecurityException) {
            KioskMode.IMMERSIVE_FALLBACK
        }
    }

    fun deactivate() {
        val activityManager = activity.getSystemService(ActivityManager::class.java)
        if (activityManager.lockTaskModeState != ActivityManager.LOCK_TASK_MODE_NONE) {
            try {
                activity.stopLockTask()
            } catch (_: SecurityException) {
                // A device owner can retain control even after local credential confirmation.
            }
        }
        activity.window.clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)
        showSystemUi()
    }

    fun hideSystemUi() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            activity.window.insetsController?.apply {
                hide(WindowInsets.Type.systemBars())
                systemBarsBehavior =
                    WindowInsetsController.BEHAVIOR_SHOW_TRANSIENT_BARS_BY_SWIPE
            }
        } else {
            @Suppress("DEPRECATION")
            activity.window.decorView.systemUiVisibility =
                View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY or
                    View.SYSTEM_UI_FLAG_FULLSCREEN or
                    View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
        }
    }

    private fun showSystemUi() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            activity.window.insetsController?.show(WindowInsets.Type.systemBars())
        } else {
            @Suppress("DEPRECATION")
            activity.window.decorView.systemUiVisibility = View.SYSTEM_UI_FLAG_VISIBLE
        }
    }

    enum class KioskMode {
        DEVICE_OWNER,
        PINNING_REQUESTED,
        ALREADY_LOCKED,
        IMMERSIVE_FALLBACK
    }

    companion object {
        fun isEnabled(context: Context): Boolean =
            context.getSharedPreferences(LaunchStatePreferences.NAME, Context.MODE_PRIVATE)
                .getBoolean(LaunchStatePreferences.KIOSK_ENABLED, false)

        fun setEnabled(context: Context, enabled: Boolean) {
            context.getSharedPreferences(LaunchStatePreferences.NAME, Context.MODE_PRIVATE)
                .edit().putBoolean(LaunchStatePreferences.KIOSK_ENABLED, enabled).apply()
        }
    }
}
