plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
}

fun quotedBuildConfig(value: String): String =
    "\"${value.replace("\\", "\\\\").replace("\"", "\\\"")}\""

val vennuVersionCode = providers.gradleProperty("vennuVersionCode")
    .orElse(providers.environmentVariable("VENNU_VERSION_CODE"))
    .orElse("1")
val vennuVersionName = providers.gradleProperty("vennuVersionName")
    .orElse(providers.environmentVariable("VENNU_VERSION_NAME"))
    .orElse("0.1.0")
val vennuBaseUrl = providers.gradleProperty("vennuBaseUrl")
    .orElse(providers.environmentVariable("VENNU_BASE_URL"))
    .orElse("https://display.vennu.app")

android {
    namespace = "com.vennu.tv"
    compileSdk = 35

    defaultConfig {
        applicationId = "com.vennu.tv"
        minSdk = 26
        targetSdk = 35
        versionCode = vennuVersionCode.get().toInt()
        versionName = vennuVersionName.get()

        buildConfigField("String", "VENNU_BASE_URL", quotedBuildConfig(vennuBaseUrl.get()))
    }

    flavorDimensions += "distribution"
    productFlavors {
        create("googleTv") {
            dimension = "distribution"
            applicationIdSuffix = ".googletv"
            buildConfigField("String", "TV_PLATFORM", quotedBuildConfig("android_tv"))
            manifestPlaceholders["appLabel"] = "Vennu TV"
        }
        create("fireTv") {
            dimension = "distribution"
            applicationIdSuffix = ".firetv"
            buildConfigField("String", "TV_PLATFORM", quotedBuildConfig("fire_tv"))
            manifestPlaceholders["appLabel"] = "Vennu Fire TV"
        }
    }

    buildFeatures {
        buildConfig = true
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    kotlinOptions {
        jvmTarget = "17"
    }
}

dependencies {
    implementation("androidx.activity:activity-ktx:1.10.0")
    implementation("androidx.core:core-ktx:1.15.0")
    implementation("androidx.webkit:webkit:1.12.1")
}
