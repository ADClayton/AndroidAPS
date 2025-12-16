import java.io.ByteArrayOutputStream
import java.text.SimpleDateFormat
import java.util.Date

plugins {
    alias(libs.plugins.ksp)
    id("com.android.application")
    id("kotlin-android")
    id("com.google.gms.google-services")
    id("com.google.firebase.crashlytics")
    id("android-app-dependencies")
    id("test-app-dependencies")
    id("jacoco-app-dependencies")
}

repositories {
    mavenCentral()
    google()
}

fun generateGitBuild(): String {
    return try {
        val processBuilder = ProcessBuilder("git", "describe", "--always")
        val output = File.createTempFile("git-build", "")
        processBuilder.redirectOutput(output)
        val process = processBuilder.start()
        process.waitFor()
        output.readText().trim()
    } catch (_: Exception) {
        "NoGitSystemAvailable"
    }
}

fun generateGitRemote(): String {
    return try {
        val processBuilder = ProcessBuilder("git", "remote", "get-url", "origin")
        val output = File.createTempFile("git-remote", "")
        processBuilder.redirectOutput(output)
        val process = processBuilder.start()
        process.waitFor()
        output.readText().trim()
    } catch (_: Exception) {
        "NoGitSystemAvailable"
    }
}

fun generateDate(): String {
    val stringBuilder = StringBuilder()
    stringBuilder.append(SimpleDateFormat("yyyy.MM.dd").format(Date()))
    return stringBuilder.toString()
}

fun isMaster(): Boolean = !Versions.appVersion.contains("-")

fun gitAvailable(): Boolean {
    return try {
        val processBuilder = ProcessBuilder("git", "--version")
        val output = File.createTempFile("git-version", "")
        processBuilder.redirectOutput(output)
        val process = processBuilder.start()
        process.waitFor()
        output.readText().isNotEmpty()
    } catch (_: Exception) {
        false
    }
}

fun allCommitted(): Boolean {
    return try {
        val processBuilder = ProcessBuilder("git", "status", "-s")
        val output = File.createTempFile("git-comited", "")
        processBuilder.redirectOutput(output)
        val process = processBuilder.start()
        process.waitFor()
        output.readText()
            .replace(Regex("""(?m)^\s*(M|A|D|\?\?)\s*.*?\.idea\/codeStyles\/.*?\s*$"""), "")
            .replace(Regex("""(?m)^\s*(\?\?)\s*.*?\s*$"""), "")
            .trim()
            .isEmpty()
    } catch (_: Exception) {
        false
    }
}

android {

    namespace = "app.aaps"
    ndkVersion = Versions.ndkVersion

    defaultConfig {
        applicationId = "info.nightscout.aapscutdown"
        minSdk = Versions.minSdk
        targetSdk = Versions.targetSdk

        buildConfigField("String", "VERSION", "\"$version\"")
        buildConfigField("String", "BUILDVERSION", "\"${generateGitBuild()}-${generateDate()}\"")
        buildConfigField("String", "REMOTE", "\"${generateGitRemote()}\"")
        buildConfigField("String", "HEAD", "\"${generateGitBuild()}\"")
        buildConfigField("String", "COMMITTED", "\"${allCommitted()}\"")

        resValue("string", "app_name", "AAPS Cutdown")
        versionName = Versions.appVersion + "-cutdown"
        manifestPlaceholders["appIcon"] = "@mipmap/ic_launcher"
        manifestPlaceholders["appIconRound"] = "@mipmap/ic_launcher_round"

        testInstrumentationRunner = "app.aaps.runners.InjectedTestRunner"
    }

    sourceSets {
        getByName("main") {
            manifest.srcFile("../app/src/main/AndroidManifest.xml")
            java.srcDir("../app/src/main/kotlin")
            java.srcDir("src/main/kotlin")
            java.exclude("app/aaps/di/AppComponent.kt")
            java.exclude("app/aaps/di/PluginsListModule.kt")
            res.srcDir("../app/src/main/res")
            assets.srcDir("../app/src/main/assets")
        }
        getByName("androidTest") {
            java.srcDir("../app/src/androidTest/kotlin")
            assets.srcDir("../app/src/androidTest/assets")
        }
    }

    useLibrary("org.apache.http.legacy")

    buildFeatures {
        dataBinding = true
        buildConfig = true
    }
}

dependencies {
    implementation(project(":shared:impl"))
    implementation(project(":core:data"))
    implementation(project(":core:objects"))
    implementation(project(":core:graph"))
    implementation(project(":core:graphview"))
    implementation(project(":core:interfaces"))
    implementation(project(":core:keys"))
    implementation(project(":core:libraries"))
    implementation(project(":core:nssdk"))
    implementation(project(":core:utils"))
    implementation(project(":core:ui"))
    implementation(project(":core:validators"))
    implementation(project(":ui"))
    implementation(project(":plugins:configuration"))
    implementation(project(":plugins:insulin"))
    implementation(project(":plugins:main"))
    implementation(project(":plugins:smoothing"))
    implementation(project(":plugins:source"))
    implementation(project(":implementation"))
    implementation(project(":database:impl"))
    implementation(project(":database:persistence"))
    implementation(project(":pump:pump-common"))
    implementation(project(":pump:omnipod-common"))
    implementation(project(":pump:omnipod-dash"))
    implementation(project(":workflow"))

    testImplementation(project(":shared:tests"))
    androidTestImplementation(project(":shared:tests"))
    androidTestImplementation(libs.androidx.test.rules)
    androidTestImplementation(libs.org.skyscreamer.jsonassert)

    kspAndroidTest(libs.com.google.dagger.android.processor)
    ksp(libs.com.google.dagger.android.processor)
    ksp(libs.com.google.dagger.compiler)

    api(libs.com.uber.rxdogtag2.rxdogtag)
    api(libs.com.google.firebase.config)
}

println("-------------------")
println("isMaster: ${isMaster()}")
println("gitAvailable: ${gitAvailable()}")
println("allCommitted: ${allCommitted()}")
println("-------------------")
if (!gitAvailable()) {
    throw GradleException("GIT system is not available. On Windows try to run Android Studio as an Administrator. Check if GIT is installed and Studio have permissions to use it")
}
if (isMaster() && !allCommitted()) {
    throw GradleException("There are uncommitted changes. Clone sources again as described in wiki and do not allow gradle update")
}
