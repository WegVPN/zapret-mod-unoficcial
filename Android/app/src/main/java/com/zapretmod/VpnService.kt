package com.zapretmod

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.content.Intent
import android.net.VpnService
import android.os.Build
import android.os.ParcelFileDescriptor
import androidx.core.app.NotificationCompat

class VpnService : VpnService() {
    private var vpnFd: ParcelFileDescriptor? = null

    override fun onCreate() {
        super.onCreate()
        createChannel()
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        when (intent?.getStringExtra("action")) {
            "start" -> startVpn()
            "stop" -> stopVpn()
        }
        return START_STICKY
    }

    private fun startVpn() {
        val builder = Builder()
            .setSessionName("ZapretMod")
            .addAddress("10.0.0.1", 24)
            .addDnsServer("1.1.1.1")
            .addDnsServer("1.0.0.1")
            .setMtu(1500)
            .addRoute("0.0.0.0", 0)
        
        // Оптимизация - разрешаем только нужные приложения
        listOf(
            "com.discord", "com.aliucord",
            "com.google.android.youtube", "com.google.android.apps.youtube.music",
            "org.telegram.messenger", "org.telegram.plus", "org.thunderdog.challegram",
            "com.vivaldi.browser", "com.android.chrome", "org.mozilla.firefox"
        ).forEach { pkg ->
            try { builder.addAllowedApplication(pkg) } catch (e: Exception) {}
        }
        
        vpnFd = builder.establish()
        showNotification()
    }

    private fun stopVpn() {
        vpnFd?.close()
        vpnFd = null
        stopForeground(STOP_FOREGROUND_REMOVE)
        stopSelf()
    }

    private fun createChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel("vpn", "ZapretMod VPN", NotificationManager.IMPORTANCE_LOW)
            getSystemService(NotificationManager::class.java).createNotificationChannel(channel)
        }
    }

    private fun showNotification() {
        val intent = Intent(this, MainActivity::class.java)
        val pi = PendingIntent.getActivity(this, 0, intent, PendingIntent.FLAG_IMMUTABLE or PendingIntent.FLAG_UPDATE_CURRENT)
        val notification = NotificationCompat.Builder(this, "vpn")
            .setContentTitle("ZapretMod")
            .setContentText("VPN активен - защита работает")
            .setSmallIcon(android.R.drawable.ic_dialog_info)
            .setContentIntent(pi)
            .setOngoing(true)
            .build()
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            startForeground(1, notification, android.content.pm.ServiceInfo.FOREGROUND_SERVICE_TYPE_SPECIAL_USE)
        } else {
            startForeground(1, notification)
        }
    }

    override fun onDestroy() { stopVpn(); super.onDestroy() }
}
