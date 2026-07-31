package com.vennu.tv

import android.content.Context

object LaunchStatePreferences {
    const val NAME = "vennu-tv-launch-state"
    const val SCREEN_ID = "screen-id"
    const val BOOT_LAUNCH_ENABLED = "boot-launch-enabled"

    fun isBootLaunchEnabled(context: Context): Boolean =
        context.getSharedPreferences(NAME, Context.MODE_PRIVATE)
            .getBoolean(BOOT_LAUNCH_ENABLED, false)
}
