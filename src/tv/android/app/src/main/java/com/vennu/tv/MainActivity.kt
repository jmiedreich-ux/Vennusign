package com.vennu.tv

import android.annotation.SuppressLint
import android.app.AlertDialog
import android.app.KeyguardManager
import android.app.Activity
import android.content.Intent
import android.graphics.Color
import android.net.ConnectivityManager
import android.net.Network
import android.net.Uri
import android.os.Bundle
import android.os.SystemClock
import android.view.Gravity
import android.view.KeyEvent
import android.view.View
import android.webkit.WebResourceError
import android.webkit.WebResourceRequest
import android.webkit.WebView
import android.webkit.WebViewClient
import android.widget.Button
import android.widget.FrameLayout
import android.widget.LinearLayout
import android.widget.ProgressBar
import android.widget.TextView
import androidx.activity.ComponentActivity
import androidx.activity.result.contract.ActivityResultContracts
import androidx.core.view.setPadding
import androidx.webkit.WebViewCompat
import androidx.webkit.WebViewFeature
import org.json.JSONObject
import java.util.UUID

class MainActivity : ComponentActivity() {
    private lateinit var webView: WebView
    private lateinit var loading: ProgressBar
    private lateinit var errorPanel: LinearLayout
    private lateinit var retryButton: Button
    private val allowedOrigin by lazy { Uri.parse(BuildConfig.VENNU_BASE_URL) }
    private val launchState by lazy {
        getSharedPreferences(LaunchStatePreferences.NAME, MODE_PRIVATE)
    }
    private val connectivityManager by lazy {
        getSystemService(ConnectivityManager::class.java)
    }
    private val kioskController by lazy { KioskController(this) }
    private var playerState = PlayerState.LOADING
    private var automaticReloads = 0
    private var lastAutomaticReloadAt = 0L
    private var backgroundedAt = 0L
    private var networkCallbackRegistered = false
    private val networkCallback = object : ConnectivityManager.NetworkCallback() {
        override fun onAvailable(network: Network) {
            runOnUiThread {
                if (playerState == PlayerState.ERROR) requestAutomaticRecovery()
            }
        }
    }
    private val operatorExitLauncher = registerForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        if (result.resultCode == Activity.RESULT_OK) exitKiosk()
    }

    @SuppressLint("SetJavaScriptEnabled")
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        webView = WebView(this).apply {
            setBackgroundColor(Color.BLACK)
            settings.javaScriptEnabled = true
            settings.domStorageEnabled = true
            settings.mediaPlaybackRequiresUserGesture = false
            isFocusable = true
            isFocusableInTouchMode = true
            webViewClient = ShellWebViewClient()
        }

        if (WebViewFeature.isFeatureSupported(WebViewFeature.DOCUMENT_START_SCRIPT)) {
            WebViewCompat.addDocumentStartJavaScript(
                webView,
                platformBridgeScript(),
                setOf(allowedOrigin.toString())
            )
        }

        loading = ProgressBar(this)
        retryButton = Button(this).apply {
            text = getString(R.string.retry)
            isFocusable = true
            isFocusableInTouchMode = true
            setOnClickListener { loadPlayer(manual = true) }
        }
        errorPanel = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            gravity = Gravity.CENTER
            setPadding(48)
            addView(TextView(context).apply {
                text = getString(R.string.load_error)
                setTextColor(Color.WHITE)
                textSize = 24f
                gravity = Gravity.CENTER
            })
            addView(retryButton)
        }

        setContentView(FrameLayout(this).apply {
            setBackgroundColor(Color.BLACK)
            addView(webView, fillParent())
            addView(loading, centered())
            addView(errorPanel, fillParent())
        })

        loadPlayer()
    }

    override fun onStart() {
        super.onStart()
        webView.onResume()
        if (KioskController.isEnabled(this)) kioskController.activate()
        if (!networkCallbackRegistered) {
            connectivityManager.registerDefaultNetworkCallback(networkCallback)
            networkCallbackRegistered = true
        }
    }

    override fun onWindowFocusChanged(hasFocus: Boolean) {
        super.onWindowFocusChanged(hasFocus)
        if (hasFocus && KioskController.isEnabled(this)) kioskController.hideSystemUi()
    }

    override fun onKeyDown(keyCode: Int, event: KeyEvent): Boolean {
        if (keyCode == KeyEvent.KEYCODE_BACK && event.repeatCount == 0 &&
            KioskController.isEnabled(this)
        ) {
            event.startTracking()
            return true
        }
        return super.onKeyDown(keyCode, event)
    }

    override fun onKeyLongPress(keyCode: Int, event: KeyEvent): Boolean {
        if (keyCode == KeyEvent.KEYCODE_BACK && KioskController.isEnabled(this)) {
            requestOperatorExit()
            return true
        }
        return super.onKeyLongPress(keyCode, event)
    }

    override fun onResume() {
        super.onResume()
        val timeAway = SystemClock.elapsedRealtime() - backgroundedAt
        if (backgroundedAt > 0L && timeAway >= STALE_FOREGROUND_MS) {
            requestAutomaticRecovery()
        }
        backgroundedAt = 0L
    }

    override fun onStop() {
        backgroundedAt = SystemClock.elapsedRealtime()
        webView.onPause()
        if (networkCallbackRegistered) {
            connectivityManager.unregisterNetworkCallback(networkCallback)
            networkCallbackRegistered = false
        }
        super.onStop()
    }

    private fun platformBridgeScript(): String =
        buildString {
            append(
                """
        Object.defineProperty(window, "__VENNU_PLATFORM__", {
          configurable: false,
          value: Object.freeze({
            platform: ${JSONObject.quote(BuildConfig.TV_PLATFORM)},
            appVersion: ${JSONObject.quote(BuildConfig.VERSION_NAME)}
        """.trimIndent()
            )
            readScreenId()?.let { append(",\n    screenId: ${JSONObject.quote(it)}") }
            append(
                """

          })
        });
        """.trimIndent()
            )
        }

    private fun loadPlayer(manual: Boolean = false) {
        if (manual) automaticReloads = 0
        showLoading()
        val screenPath = readScreenId()?.let { "/display/${Uri.encode(it)}" } ?: "/pair"
        webView.loadUrl("${BuildConfig.VENNU_BASE_URL}$screenPath")
    }

    private fun requestAutomaticRecovery() {
        val now = SystemClock.elapsedRealtime()
        if (automaticReloads >= MAX_AUTOMATIC_RELOADS) return
        if (lastAutomaticReloadAt > 0L &&
            now - lastAutomaticReloadAt < AUTOMATIC_RELOAD_COOLDOWN_MS
        ) return

        automaticReloads += 1
        lastAutomaticReloadAt = now
        loadPlayer()
    }

    private fun showLoading() {
        playerState = PlayerState.LOADING
        webView.visibility = View.INVISIBLE
        errorPanel.visibility = View.GONE
        loading.visibility = View.VISIBLE
    }

    private fun showPlayer() {
        playerState = PlayerState.READY
        automaticReloads = 0
        loading.visibility = View.GONE
        errorPanel.visibility = View.GONE
        webView.visibility = View.VISIBLE
        webView.requestFocus()
    }

    private fun showError() {
        playerState = PlayerState.ERROR
        loading.visibility = View.GONE
        webView.visibility = View.INVISIBLE
        errorPanel.visibility = View.VISIBLE
        retryButton.requestFocus()
    }

    private fun isAllowed(url: Uri): Boolean =
        url.scheme == "https" &&
            url.host.equals(allowedOrigin.host, ignoreCase = true) &&
            effectivePort(url) == effectivePort(allowedOrigin)

    private fun effectivePort(url: Uri): Int =
        if (url.port == -1) 443 else url.port

    private fun recordTrustedNavigation(url: Uri) {
        if (!isAllowed(url)) return

        if (url.path == "/pair" && url.getQueryParameter(RESET_QUERY) == "1") {
            launchState.edit().remove(LaunchStatePreferences.SCREEN_ID).apply()
            return
        }

        if (url.path == "/pair") {
            when (url.getQueryParameter(BOOT_QUERY)) {
                "1" -> launchState.edit()
                    .putBoolean(LaunchStatePreferences.BOOT_LAUNCH_ENABLED, true).apply()
                "0" -> launchState.edit()
                    .putBoolean(LaunchStatePreferences.BOOT_LAUNCH_ENABLED, false).apply()
            }
            when (url.getQueryParameter(KIOSK_QUERY)) {
                "1" -> KioskController.setEnabled(this, true)
                "0" -> KioskController.setEnabled(this, false)
            }
        }

        val encodedScreenId = DISPLAY_PATH.matchEntire(url.path.orEmpty())?.groupValues?.get(1)
            ?: return
        val screenId = normalizeScreenId(Uri.decode(encodedScreenId)) ?: return
        launchState.edit().putString(LaunchStatePreferences.SCREEN_ID, screenId).apply()
    }

    private fun readScreenId(): String? {
        val stored = launchState.getString(LaunchStatePreferences.SCREEN_ID, null) ?: return null
        val normalized = normalizeScreenId(stored)
        if (normalized == null) {
            launchState.edit().remove(LaunchStatePreferences.SCREEN_ID).apply()
        }
        return normalized
    }

    private fun normalizeScreenId(value: String): String? =
        try {
            UUID.fromString(value.trim()).toString()
        } catch (_: IllegalArgumentException) {
            null
        }

    private fun requestOperatorExit() {
        val keyguard = getSystemService(KeyguardManager::class.java)
        if (!keyguard.isDeviceSecure) {
            AlertDialog.Builder(this)
                .setTitle(R.string.operator_exit_title)
                .setMessage(R.string.operator_exit_unavailable)
                .setPositiveButton(R.string.ok, null)
                .show()
            return
        }

        val confirmation = keyguard.createConfirmDeviceCredentialIntent(
            getString(R.string.operator_exit_title),
            getString(R.string.operator_exit_prompt)
        )
        if (confirmation != null) operatorExitLauncher.launch(confirmation)
    }

    private fun exitKiosk() {
        KioskController.setEnabled(this, false)
        kioskController.deactivate()
        startActivity(
            Intent(Intent.ACTION_MAIN).apply {
                addCategory(Intent.CATEGORY_HOME)
                flags = Intent.FLAG_ACTIVITY_NEW_TASK
            }
        )
        finish()
    }

    private inner class ShellWebViewClient : WebViewClient() {
        override fun shouldOverrideUrlLoading(view: WebView, request: WebResourceRequest): Boolean =
            !isAllowed(request.url)

        override fun onPageStarted(view: WebView, url: String, favicon: android.graphics.Bitmap?) {
            val parsedUrl = Uri.parse(url)
            if (isAllowed(parsedUrl)) {
                recordTrustedNavigation(parsedUrl)
                showLoading()
            } else {
                showError()
            }
        }

        override fun onPageCommitVisible(view: WebView, url: String) {
            if (isAllowed(Uri.parse(url))) showPlayer() else showError()
        }

        override fun onReceivedError(
            view: WebView,
            request: WebResourceRequest,
            error: WebResourceError
        ) {
            if (request.isForMainFrame) showError()
        }
    }

    private fun fillParent() = FrameLayout.LayoutParams(
        FrameLayout.LayoutParams.MATCH_PARENT,
        FrameLayout.LayoutParams.MATCH_PARENT
    )

    private fun centered() = FrameLayout.LayoutParams(
        FrameLayout.LayoutParams.WRAP_CONTENT,
        FrameLayout.LayoutParams.WRAP_CONTENT,
        Gravity.CENTER
    )

    companion object {
        private const val RESET_QUERY = "vennuReset"
        private const val BOOT_QUERY = "vennuBoot"
        private const val KIOSK_QUERY = "vennuKiosk"
        private const val STALE_FOREGROUND_MS = 5 * 60 * 1000L
        private const val AUTOMATIC_RELOAD_COOLDOWN_MS = 10_000L
        private const val MAX_AUTOMATIC_RELOADS = 3
        private val DISPLAY_PATH = Regex("^/display/([^/]+)/?$")
    }

    private enum class PlayerState { LOADING, READY, ERROR }
}
