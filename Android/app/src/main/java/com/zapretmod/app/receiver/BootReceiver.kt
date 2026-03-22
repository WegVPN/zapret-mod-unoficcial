package com.zapretmod.app.receiver

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import com.zapretmod.app.service.ZapretVpnService
import mu.KotlinLogging

private val logger = KotlinLogging.logger {}

/**
 * Boot receiver - auto-start VPN on device boot (if enabled in settings)
 */
class BootReceiver : BroadcastReceiver() {
    
    override fun onReceive(context: Context, intent: Intent) {
        if (intent.action == Intent.ACTION_BOOT_COMPLETED ||
            intent.action == "android.intent.action.QUICKBOOT_POWERON") {
            
            logger.info { "Boot completed - checking auto-start settings" }
            
            // Check if auto-start is enabled in preferences
            val prefs = context.getSharedPreferences("zapretmod_prefs", Context.MODE_PRIVATE)
            val autoStartEnabled = prefs.getBoolean("auto_start", false)
            
            if (autoStartEnabled) {
                logger.info { "Auto-start enabled - starting VPN service" }
                
                val serviceIntent = Intent(context, ZapretVpnService::class.java).apply {
                    putExtra("ACTION", "START")
                    putExtra("STRATEGY", prefs.getString("last_strategy", "all") ?: "all")
                }
                
                try {
                    if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                        context.startForegroundService(serviceIntent)
                    } else {
                        context.startService(serviceIntent)
                    }
                } catch (e: Exception) {
                    logger.error(e) { "Failed to start VPN service on boot" }
                }
            } else {
                logger.debug { "Auto-start disabled - not starting VPN" }
            }
        }
    }
}
