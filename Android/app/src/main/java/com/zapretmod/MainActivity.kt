package com.zapretmod

import android.app.Activity
import android.content.Intent
import android.net.VpnService
import android.os.Build
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

class MainActivity : ComponentActivity() {
    private var vpnRunning by mutableStateOf(false)
    private val vpnLauncher = registerForActivityResult(ActivityResultContracts.StartActivityForResult()) {
        if (it.resultCode == Activity.RESULT_OK) { startVpn() }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            Surface(modifier = Modifier.fillMaxSize(), color = Color(0xFF1E1E2E)) {
                Column(modifier = Modifier.fillMaxSize().padding(20.dp), horizontalAlignment = Alignment.CenterHorizontally) {
                    Text("🛡 ZapretMod", fontSize = 28.sp, color = Color.White, modifier = Modifier.padding(top = 40.dp))
                    Text("DPI Bypass для Discord, YouTube, Telegram", fontSize = 14.sp, color = Color.Gray)
                    Spacer(modifier = Modifier.height(40.dp))
                    Card(modifier = Modifier.fillMaxWidth().padding(vertical = 8.dp), shape = RoundedCornerShape(12.dp), colors = CardDefaults.cardColors(containerColor = Color(0xFF2A2A3A))) {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Text("📋 Стратегия", fontSize = 16.sp, color = Color.White)
                            Spacer(modifier = Modifier.height(8.dp))
                            Text("Discord + YouTube + Telegram", fontSize = 14.sp, color = Color(0xFFA0A0B0))
                        }
                    }
                    Card(modifier = Modifier.fillMaxWidth().padding(vertical = 8.dp), shape = RoundedCornerShape(12.dp), colors = CardDefaults.cardColors(containerColor = Color(0xFF2A2A3A))) {
                        Column(modifier = Modifier.padding(16.dp)) {
                            Text("⚙ Опции", fontSize = 16.sp, color = Color.White)
                            Spacer(modifier = Modifier.height(8.dp))
                            Text("✓ Оптимизация сети", fontSize = 14.sp, color = Color(0xFF00FF88))
                            Text("✓ DNS: 1.1.1.1", fontSize = 14.sp, color = Color(0xFF00FF88))
                        }
                    }
                    Spacer(modifier = Modifier.weight(1f))
                    Button(onClick = { toggleVpn() }, modifier = Modifier.fillMaxWidth().height(56.dp), shape = RoundedCornerShape(12.dp), colors = ButtonDefaults.buttonColors(containerColor = if (vpnRunning) Color(0xFFDA373C) else Color(0xFF5865F2))) {
                        Text(if (vpnRunning) "⏹ ОСТАНОВИТЬ" else "▶ ЗАПУСТИТЬ", fontSize = 18.sp)
                    }
                    Spacer(modifier = Modifier.height(20.dp))
                }
            }
        }
    }

    private fun toggleVpn() {
        if (vpnRunning) { stopVpn() } else {
            val intent = VpnService.prepare(this)
            if (intent != null) vpnLauncher.launch(intent) else startVpn()
        }
    }

    private fun startVpn() {
        vpnRunning = true
        val intent = Intent(this, VpnService::class.java).apply { putExtra("action", "start") }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) startForegroundService(intent) else startService(intent)
    }

    private fun stopVpn() {
        vpnRunning = false
        stopService(Intent(this, VpnService::class.java))
    }
}
