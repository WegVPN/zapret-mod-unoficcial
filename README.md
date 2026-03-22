# 🛡 ZapretMod v3.0 - DPI Bypass

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Windows](https://img.shields.io/badge/Windows-10%2F11%20x64-0078D6)]()
[![Android](https://img.shields.io/badge/Android-8.0+-3DDC84)]()
[![Release](https://img.shields.io/github/v/release/WegVPN/zapret-mod-unoficcial)]()

**ZapretMod** — готовое решение для обхода DPI блокировок Discord, YouTube, Telegram.

## 📥 Скачать

| Платформа | Ссылка | Размер |
|-----------|--------|--------|
| **Windows** | [Скачать ZIP](https://github.com/WegVPN/zapret-mod-unoficcial/releases/download/v3.0/ZapretMod-v3.0-win-x64.zip) | 555 KB |
| **Android** | [Скачать APK](https://github.com/WegVPN/zapret-mod-unoficcial/releases/download/v3.0/ZapretMod-v3.0.apk) | ~5 MB |

## 🚀 Быстрый старт

### Windows
1. **Распакуйте** ZIP архив
2. **Запустите** `ZapretMod.exe` от имени администратора
3. **Нажмите** "📥 Скачать файлы"
4. **Нажмите** "▶ ЗАПУСТИТЬ"

### Android
1. **Установите** APK файл
2. **Откройте** ZapretMod
3. **Нажмите** "▶ ЗАПУСТИТЬ"
4. **Разрешите** VPN подключение

## ✨ Возможности

### Windows
- 🎨 Современный WPF GUI (Discord цвета)
- 📋 6 стратегий обхода DPI
- 📥 Автозагрузка файлов
- 🚀 Оптимизация интернета
- 🔍 Диагностика
- 📊 Системный трей

### Android
- 📱 Material Design 3 UI
- 🔒 VPN Service
- 🎨 Jetpack Compose
- 🌐 DNS 1.1.1.1
- 🔔 Уведомления
- ⚡ Оптимизация сети

## 📋 Стратегии

| Стратегия | Описание | Для чего |
|-----------|----------|----------|
| **Discord + YouTube + Telegram** | Основная | Рекомендуется |
| **Discord Only** | Только Discord | Голосовые каналы |
| **YouTube Only** | Только YouTube | 4K стриминг |
| **Telegram Only** | Только Telegram | Мессенджер |
| **FAKE TLS AUTO** | Авто TLS | Универсальная |
| **SIMPLE FAKE** | Простая | Мин. задержки |

## ⚙ Требования

### Windows
- Windows 10/11 x64
- .NET 8 Desktop Runtime
- Права администратора

### Android
- Android 8.0+ (API 26+)
- Разрешение VPN

## 🔧 Решение проблем

### Windows
| Проблема | Решение |
|----------|---------|
| "Файлы не найдены" | Нажмите "📥 Скачать файлы" |
| "Отказано в доступе" | Запустите от администратора |
| Не работает Discord | Попробуйте "FAKE TLS AUTO" |
| Антивирус блокирует | Добавьте в исключения |

### Android
| Проблема | Решение |
|----------|---------|
| Не подключается VPN | Разрешите в настройках |
| Закрывается | Проверьте Android 8.0+ |
| Нет интернета | Отключите и включите снова |

## 📁 Структура проекта

```
zapret-mod-unoficcial/
├── ZapretMod/              # Windows версия
│   ├── Core/
│   │   └── ZapretEngine.cs # Движок
│   ├── App.xaml            # Стили
│   └── MainWindow.xaml     # GUI
├── Android/                # Android версия
│   └── app/
│       └── src/main/
│           ├── java/com/zapretmod/
│           │   ├── MainActivity.kt
│           │   └── VpnService.kt
│           └── res/        # Ресурсы
└── README.md
```

## 🔧 Как это работает

### Windows
```
ZapretMod GUI → ZapretEngine → winws.exe → WinDivert → Сеть
                                         ↓
                                  DPI обход
```

### Android
```
ZapretMod UI → VpnService → Android VPN API → Сеть
                                ↓
                         DNS 1.1.1.1 + оптимизация
```

## 📄 Лицензия

MIT License

## 📞 Ссылки

- **GitHub**: https://github.com/WegVPN/zapret-mod-unoficcial
- **Оригинал**: https://github.com/flowseal/zapret-discord-youtube
- **Zapret**: https://github.com/bol-van/zapret

---

**ZapretMod v3.0** © 2024. Вдохновлено flowseal/zapret-discord-youtube.
