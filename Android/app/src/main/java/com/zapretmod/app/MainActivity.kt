package com.zapretmod.app

import android.content.ComponentName
import android.content.Context
import android.content.Intent
import android.content.ServiceConnection
import android.net.VpnService
import android.os.Bundle
import android.os.IBinder
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.zapretmod.app.service.ZapretVpnService
import com.zapretmod.app.ui.theme.ZapretModTheme
import mu.KotlinLogging

private val logger = KotlinLogging.logger {}

class MainActivity : ComponentActivity() {

    private var vpnService: ZapretVpnService? = null
    private var isBound by mutableStateOf(false)
    private var isVpnRunning by mutableStateOf(false)
    private var selectedStrategy by mutableStateOf("all")

    private val connection = object : ServiceConnection {
        override fun onServiceConnected(name: ComponentName?, service: IBinder?) {
            val binder = service as ZapretVpnService.LocalBinder
            vpnService = binder.getService()
            isBound = true
            logger.info { "Service connected" }
        }

        override fun onServiceDisconnected(name: ComponentName?) {
            vpnService = null
            isBound = false
            logger.info { "Service disconnected" }
        }
    }

    private val vpnPermissionLauncher = registerForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        if (result.resultCode == RESULT_OK) {
            startVpnService()
        } else {
            logger.warn { "VPN permission denied" }
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            ZapretModTheme {
                MainScreen(
                    isVpnRunning = isVpnRunning,
                    selectedStrategy = selectedStrategy,
                    onToggleVpn = { toggleVpn() },
                    onSelectStrategy = { selectedStrategy = it }
                )
            }
        }
    }

    override fun onStart() {
        super.onStart()
        Intent(this, ZapretVpnService::class.java).also { intent ->
            bindService(intent, connection, Context.BIND_AUTO_CREATE)
        }
    }

    override fun onStop() {
        super.onStop()
        unbindService(connection)
        isBound = false
    }

    private fun toggleVpn() {
        if (isVpnRunning) {
            stopVpnService()
        } else {
            requestVpnPermission()
        }
    }

    private fun requestVpnPermission() {
        val intent = VpnService.prepare(this)
        if (intent != null) {
            vpnPermissionLauncher.launch(intent)
        } else {
            startVpnService()
        }
    }

    private fun startVpnService() {
        Intent(this, ZapretVpnService::class.java).apply {
            putExtra("ACTION", "START")
            putExtra("STRATEGY", selectedStrategy)
        }.also {
            if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                startForegroundService(it)
            } else {
                startService(it)
            }
        }
        isVpnRunning = true
    }

    private fun stopVpnService() {
        Intent(this, ZapretVpnService::class.java).apply {
            putExtra("ACTION", "STOP")
        }.also {
            stopService(it)
        }
        isVpnRunning = false
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun MainScreen(
    isVpnRunning: Boolean,
    selectedStrategy: String,
    onToggleVpn: () -> Unit,
    onSelectStrategy: (String) -> Unit
) {
    val strategies = listOf(
        StrategyItem("all", "All Services", Icons.Default.Dns),
        StrategyItem("discord", "Discord", Icons.Default.Message),
        StrategyItem("youtube", "YouTube", Icons.Default.PlayArrow),
        StrategyItem("telegram", "Telegram", Icons.Default.Telegram)
    )

    Scaffold(
        topBar = {
            TopAppBar(
                title = { 
                    Text(
                        "🛡 ZapretMod",
                        fontWeight = FontWeight.Bold
                    ) 
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = MaterialTheme.colorScheme.primaryContainer,
                    titleContentColor = MaterialTheme.colorScheme.onPrimaryContainer
                )
            )
        }
    ) { paddingValues ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            // Status Card
            StatusCard(
                isRunning = isVpnRunning,
                modifier = Modifier.fillMaxWidth()
            )

            Spacer(modifier = Modifier.height(24.dp))

            // Strategy Selection
            Text(
                text = "Select Strategy",
                fontSize = 18.sp,
                fontWeight = FontWeight.SemiBold,
                modifier = Modifier.align(Alignment.Start)
            )

            Spacer(modifier = Modifier.height(12.dp))

            strategies.forEach { strategy ->
                StrategyCard(
                    strategy = strategy,
                    isSelected = selectedStrategy == strategy.id,
                    onClick = { onSelectStrategy(strategy.id) },
                    modifier = Modifier.fillMaxWidth()
                )
            }

            Spacer(modifier = Modifier.weight(1f))

            // Toggle Button
            Button(
                onClick = onToggleVpn,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(56.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = if (isVpnRunning) 
                        MaterialTheme.colorScheme.error 
                    else 
                        MaterialTheme.colorScheme.primary
                )
            ) {
                Icon(
                    imageVector = if (isVpnRunning) Icons.Default.Stop 
                    else Icons.Default.PlayArrow,
                    contentDescription = null,
                    modifier = Modifier.padding(end = 8.dp)
                )
                Text(
                    text = if (isVpnRunning) "STOP VPN" else "START VPN",
                    fontSize = 18.sp,
                    fontWeight = FontWeight.Bold
                )
            }
        }
    }
}

@Composable
fun StatusCard(isRunning: Boolean, modifier: Modifier = Modifier) {
    Card(
        modifier = modifier,
        colors = CardDefaults.cardColors(
            containerColor = if (isRunning) 
                Color(0xFF4CD037).copy(alpha = 0.2f) 
            else 
                Color(0xFFFF3B30).copy(alpha = 0.2f)
        )
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(20.dp),
            horizontalArrangement = Arrangement.Center,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(
                modifier = Modifier
                    .size(16.dp)
                    .background(
                        color = if (isRunning) Color(0xFF4CD037) else Color(0xFFFF3B30),
                        shape = MaterialTheme.shapes.small
                    )
            )
            Spacer(modifier = Modifier.width(12.dp))
            Text(
                text = if (isRunning) "VPN Active - Protected" else "VPN Inactive",
                fontSize = 20.sp,
                fontWeight = FontWeight.Bold,
                color = if (isRunning) Color(0xFF4CD037) else Color(0xFFFF3B30)
            )
        }
    }
}

data class StrategyItem(
    val id: String,
    val name: String,
    val icon: ImageVector
)

@Composable
fun StrategyCard(
    strategy: StrategyItem,
    isSelected: Boolean,
    onClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    Card(
        modifier = modifier
            .padding(vertical = 4.dp),
        onClick = onClick,
        colors = CardDefaults.cardColors(
            containerColor = if (isSelected)
                MaterialTheme.colorScheme.primaryContainer
            else
                MaterialTheme.colorScheme.surfaceVariant
        )
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(
                imageVector = strategy.icon,
                contentDescription = null,
                tint = if (isSelected)
                    MaterialTheme.colorScheme.primary
                else
                    MaterialTheme.colorScheme.onSurfaceVariant,
                modifier = Modifier.size(28.dp)
            )
            Spacer(modifier = Modifier.width(16.dp))
            Text(
                text = strategy.name,
                fontSize = 16.sp,
                fontWeight = if (isSelected) FontWeight.SemiBold else FontWeight.Normal,
                color = if (isSelected)
                    MaterialTheme.colorScheme.onPrimaryContainer
                else
                    MaterialTheme.colorScheme.onSurfaceVariant
            )
            Spacer(modifier = Modifier.weight(1f))
            if (isSelected) {
                Icon(
                    imageVector = Icons.Default.CheckCircle,
                    contentDescription = null,
                    tint = MaterialTheme.colorScheme.primary
                )
            }
        }
    }
}
