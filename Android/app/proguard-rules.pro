# Add project specific ProGuard rules here.
# By default, the flags in this file are appended to flags specified
# in /usr/local/Cellar/android-sdk/24.3.3/tools/proguard/proguard-android.txt

# Keep logging classes
-keep class mu.** { *; }
-keep class kotlin.** { *; }

# Keep service classes
-keep class com.zapretmod.app.service.** { *; }
-keep class com.zapretmod.app.receiver.** { *; }

# Keep data classes
-keep class com.zapretmod.app.data.** { *; }

# Kotlin serialization
-keepattributes *Annotation*, InnerClasses
-dontnote kotlinx.serialization.AnnotationsKt

-keepclassmembers class kotlinx.serialization.json.** {
    *** Companion;
}
-keepclasseswithmembers class kotlinx.serialization.json.** {
    kotlinx.serialization.KSerializer serializer(...);
}

# Keep Compose
-keep class androidx.compose.** { *; }
