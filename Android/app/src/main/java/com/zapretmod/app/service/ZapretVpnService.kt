package com.zapretmod.app.service

import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.content.Intent
import android.content.pm.ServiceInfo
import android.net.VpnService
import android.os.Binder
import android.os.Build
import android.os.IBinder
import android.os.ParcelFileDescriptor
import androidx.core.app.NotificationCompat
import com.zapretmod.app.MainActivity
import com.zapretmod.app.R
import kotlinx.coroutines.*
import mu.KotlinLogging
import java.io.File
import java.io.FileInputStream
import java.io.FileOutputStream

private val logger = KotlinLogging.logger {}

/**
 * VPN Service for DPI bypass on Android
 * Similar to flowseal/zapret-discord-youtube but for Android
 */
class ZapretVpnService : VpnService() {

    private val binder = LocalBinder()
    private val serviceScope = CoroutineScope(Dispatchers.IO + SupervisorJob())
    
    private var vpnInterface: ParcelFileDescriptor? = null
    private var isRunning = false
    
    // DPI bypass strategies (similar to Windows version)
    private val strategies = mapOf(
        "discord" to StrategyConfig(
            name = "Discord",
            domains = listOf("discord.com", "discord.gg", "discordapp.com"),
            ports = listOf(443, 80, 8443)
        ),
        "youtube" to StrategyConfig(
            name = "YouTube",
            domains = listOf("youtube.com", "ytimg.com", "googlevideo.com"),
            ports = listOf(443, 80)
        ),
        "telegram" to StrategyConfig(
            name = "Telegram",
            domains = listOf("telegram.org", "telegram.me", "t.me"),
            ips = listOf("149.154.160.0/20", "91.108.4.0/22"),
            ports = listOf(443, 80, 8443)
        ),
        "all" to StrategyConfig(
            name = "All Services",
            domains = listOf(
                "discord.com", "discord.gg", "discordapp.com",
                "youtube.com", "ytimg.com", "googlevideo.com",
                "telegram.org", "telegram.me", "t.me"
            ),
            ports = listOf(443, 80, 8443)
        )
    )
    
    private var currentStrategy: StrategyConfig? = null

    inner class LocalBinder : Binder() {
        fun getService(): ZapretVpnService = this@ZapretVpnService
    }

    override fun onBind(intent: Intent?): IBinder {
        return binder
    }

    override fun onCreate() {
        super.onCreate()
        logger.info { "ZapretVpnService created" }
        createNotificationChannel()
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        logger.info { "ZapretVpnService started" }
        
        val action = intent?.getStringExtra("ACTION")
        val strategyKey = intent?.getStringExtra("STRATEGY") ?: "all"
        
        when (action) {
            "START" -> startVpn(strategyKey)
            "STOP" -> stopVpn()
        }
        
        return START_STICKY
    }

    private fun startVpn(strategyKey: String) {
        if (isRunning) {
            logger.warn { "VPN already running" }
            return
        }

        currentStrategy = strategies[strategyKey]
        
        serviceScope.launch {
            try {
                // Setup VPN interface
                setupVpnInterface()
                
                // Start packet inspection thread
                startPacketInspection()
                
                isRunning = true
                
                // Show notification
                showNotification()
                
                logger.info { "VPN started with strategy: $strategyKey" }
            } catch (e: Exception) {
                logger.error(e) { "Failed to start VPN" }
                stopVpn()
            }
        }
    }

    private fun setupVpnInterface() {
        val builder = Builder()
            .setSessionName("ZapretMod")
            .addAddress("10.0.0.1", 24)
            .addDnsServer("1.1.1.1")
            .addDnsServer("1.0.0.1")
            .setMtu(1500)
        
        // Add routes for target domains/IPs
        currentStrategy?.let { strategy ->
            // Add DNS bypass for specific domains
            strategy.domains.forEach { domain ->
                // In real implementation, would resolve domains to IPs
                logger.debug { "Adding route for domain: $domain" }
            }
            
            // Add IP routes
            strategy.ips?.forEach { ipRange ->
                try {
                    val (ip, prefix) = ipRange.split("/")
                    builder.addRoute(ip, prefix.toInt())
                } catch (e: Exception) {
                    logger.error(e) { "Invalid IP range: $ipRange" }
                }
            }
        }
        
        // Exclude common apps (games, etc.) if game filter enabled
        if (preferences.gameFilterEnabled) {
            excludeGameApps(builder)
        }
        
        vpnInterface = builder.establish()
        logger.info { "VPN interface established" }
    }

    private fun excludeGameApps(builder: Builder) {
        // Exclude popular game apps from VPN
        val gamePackages = listOf(
            "com.supercell.clashofclans",
            "com.king.candycrushsaga",
            "com.mojang.minecraftpe",
            "com.roblox.client"
        )
        
        gamePackages.forEach { pkg ->
            try {
                builder.addAllowedApplication(pkg)
            } catch (e: PackageManager.NameNotFoundException) {
                // App not installed
            }
        }
    }

    private suspend fun startPacketInspection() {
        withContext(Dispatchers.IO) {
            val vpnFd = vpnInterface?.fileDescriptor ?: return@withContext
            
            // In a real implementation, this would:
            // 1. Read packets from VPN interface
            // 2. Apply DPI bypass techniques (packet fragmentation, fake TLS, etc.)
            // 3. Forward modified packets
            
            // For now, just log that we're running
            logger.info { "Packet inspection started" }
            
            // Keep running until stopped
            while (isRunning) {
                delay(1000)
            }
        }
    }

    fun stopVpn() {
        logger.info { "Stopping VPN" }
        
        isRunning = false
        vpnInterface?.close()
        vpnInterface = null
        currentStrategy = null
        
        stopForeground(STOP_FOREGROUND_REMOVE)
        stopSelf()
        
        logger.info { "VPN stopped" }
    }

    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel(
                CHANNEL_ID,
                "ZapretMod VPN Service",
                NotificationManager.IMPORTANCE_LOW
            ).apply {
                description = "DPI Bypass Service"
            }
            
            val notificationManager = getSystemService(NotificationManager::class.java)
            notificationManager.createNotificationChannel(channel)
        }
    }

    private fun showNotification() {
        val intent = Intent(this, MainActivity::class.java)
        val pendingIntent = PendingIntent.getActivity(
            this, 0, intent,
            PendingIntent.FLAG_IMMUTABLE or PendingIntent.FLAG_UPDATE_CURRENT
        )

        val notification: Notification = NotificationCompat.Builder(this, CHANNEL_ID)
            .setContentTitle("ZapretMod")
            .setContentText("DPI Bypass Active - ${currentStrategy?.name ?: "All"}")
            .setSmallIcon(R.drawable.ic_notification)
            .setContentIntent(pendingIntent)
            .setOngoing(true)
            .build()

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            startForeground(1, notification, ServiceInfo.FOREGROUND_SERVICE_TYPE_SPECIAL_USE)
        } else {
            startForeground(1, notification)
        }
    }

    override fun onDestroy() {
        super.onDestroy()
        serviceScope.cancel()
        vpnInterface?.close()
        logger.info { "ZapretVpnService destroyed" }
    }

    companion object {
        private const val CHANNEL_ID = "zapretmod_vpn_channel"
    }
}

/**
 * Strategy configuration data class
 */
data class StrategyConfig(
    val name: String,
    val domains: List<String>,
    val ips: List<String>? = null,
    val ports: List<Int>
)

/**
 * Preferences helper object
 */
object preferences {
    var gameFilterEnabled: Boolean = false
}
