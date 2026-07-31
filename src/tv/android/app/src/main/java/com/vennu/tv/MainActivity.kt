package com.vennu.tv

import android.annotation.SuppressLint
import android.graphics.Color
import android.net.Uri
import android.os.Bundle
import android.view.Gravity
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
import androidx.core.view.setPadding
import androidx.webkit.WebViewCompat
import androidx.webkit.WebViewFeature

class MainActivity : ComponentActivity() {
    private lateinit var webView: WebView
    private lateinit var loading: ProgressBar
    private lateinit var errorPanel: LinearLayout
    private lateinit var retryButton: Button
    private val allowedOrigin by lazy { Uri.parse(BuildConfig.VENNU_BASE_URL) }

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
            setOnClickListener { loadPlayer() }
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

    private fun platformBridgeScript(): String =
        """
        Object.defineProperty(window, "__VENNU_PLATFORM__", {
          configurable: false,
          value: Object.freeze({
            platform: "${BuildConfig.TV_PLATFORM}",
            appVersion: "${BuildConfig.VERSION_NAME}"
          })
        });
        """.trimIndent()

    private fun loadPlayer() {
        showLoading()
        webView.loadUrl(BuildConfig.VENNU_BASE_URL)
    }

    private fun showLoading() {
        webView.visibility = View.INVISIBLE
        errorPanel.visibility = View.GONE
        loading.visibility = View.VISIBLE
    }

    private fun showPlayer() {
        loading.visibility = View.GONE
        errorPanel.visibility = View.GONE
        webView.visibility = View.VISIBLE
        webView.requestFocus()
    }

    private fun showError() {
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

    private inner class ShellWebViewClient : WebViewClient() {
        override fun shouldOverrideUrlLoading(view: WebView, request: WebResourceRequest): Boolean =
            !isAllowed(request.url)

        override fun onPageStarted(view: WebView, url: String, favicon: android.graphics.Bitmap?) {
            if (isAllowed(Uri.parse(url))) showLoading() else showError()
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
}
