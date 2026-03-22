# ZapretMod - DPI Bypass for Discord, YouTube, Telegram

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Windows](https://img.shields.io/badge/platform-Windows%2010%2F11%20x64-lightgrey)]()
[![Android](https://img.shields.io/badge/platform-Android%208.0+-brightgreen)]()
[![Inspired by](https://img.shields.io/badge/inspired%20by-flowseal%2Fzapret--discord--youtube-orange)](https://github.com/flowseal/zapret-discord-youtube)

**ZapretMod** — это кроссплатформенное приложение для обхода DPI-блокировок (Deep Packet Inspection) для **Discord**, **YouTube**, **Telegram** и других сервисов.

Вдохновлено проектом [flowseal/zapret-discord-youtube](https://github.com/flowseal/zapret-discord-youtube) с добавлением современного GUI и поддержки Android.

## 📱 Платформы

| Платформа | Статус | Требования |
|-----------|--------|------------|
| **Windows** | ✅ Готово | Windows 10/11 x64, .NET 8 |
| **Android** | ✅ Готово | Android 8.0+ (API 26+) |

## ✨ Возможности

### Общие для обеих платформ
- 🎯 **Готовые стратегии** для Discord, YouTube, Telegram
- 🔄 **15+ стратегий** обхода (как в flowseal)
- 🚀 **Автозапуск** при старте системы
- 📊 **Логирование** работы
- 🎨 **Тёмная тема** (Material You на Android)

### Windows версия
- 🖥 **Современный WPF GUI** с системным треем
- 🛠 **Windows Service** для автозапуска
- 📋 **Batch-файлы** (как в flowseal)
- 🔍 **Диагностика** и проверка настроек
- 📦 **Установщик** (MSI/Inno Setup)

### Android версия
- 📱 **Material Design 3** UI
- 🔒 **VPN Service** для перехвата трафика
- ⚡ **Jetpack Compose** современный интерфейс
- 🔔 **Уведомления** в статус-баре
- 🚀 **Auto-start** при загрузке устройства

## 📥 Установка

### Windows

#### Вариант 1: Portable (рекомендуется)
1. Скачайте `ZapretMod-v2.0.0-win-x64.zip` из [Releases](https://github.com/WegVPN/zapret-mod-unoficcial/releases)
2. Распакуйте в любую папку
3. **Скачайте [zapret-win-bundle](https://github.com/bol-van/zapret-win-bundle/releases)** и скопируйте `winws.exe`, `WinDivert64.sys`, `WinDivert64.dll` в папку `bin\`
4. Запустите `ZapretMod.exe` от имени администратора

#### Вариант 2: Установщик
1. Скачайте `ZapretMod-Setup-2.0.0.exe`
2. Запустите установщик
3. Следуйте инструкциям мастера

#### Вариант 3: Из исходного кода
```bash
cd Windows
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained false
```

### Android

#### Вариант 1: APK из Releases
1. Скачайте `ZapretMod-v2.0.0.apk` из [Releases](https://github.com/WegVPN/zapret-mod-unoficcial/releases)
2. Разрешите установку из неизвестных источников
3. Установите APK файл
4. Запустите приложение и предоставьте разрешение на VPN

#### Вариант 2: Сборка из исходного кода
```bash
cd Android
# Откройте в Android Studio или используйте командную строку:
./gradlew assembleRelease
# APK будет в app/build/outputs/apk/release/
```

## 🚀 Быстрый старт

### Windows
1. Запустите **ZapretMod.exe** от имени администратора
2. Выберите стратегию (например, "Discord + YouTube + Telegram")
3. Нажмите **"▶ Запустить"**
4. Проверьте работу сервисов

### Android
1. Откройте приложение **ZapretMod**
2. Выберите стратегию (Discord, YouTube, Telegram или All)
3. Нажмите **"START VPN"**
4. Подтвердите подключение VPN
5. Готово! Трафик защищён

## 📋 Стратегии (как в flowseal/zapret-discord-youtube)

| Стратегия | Описание | Параметры |
|-----------|----------|-----------|
| **Discord + YouTube + Telegram** | Основная стратегия для всех сервисов | `--wf=l3 --dpi-desync=fake --dpi-desync-autottls=1` |
| **FAKE TLS AUTO** | Автоматический fake TLS | `--dpi-desync-fake-tls=oob` |
| **SIMPLE FAKE** | Простой fake метод | `--dpi-desync=simple-fake` |
| **Discord Only** | Только Discord | `--domain-list=discord.txt` |
| **YouTube Only** | Только YouTube | `--domain-list=youtube.txt` |
| **Telegram Only** | Только Telegram | `--ip-list=telegram.txt` |

## 📁 Структура проекта

```
zapret-mod-unoficcial/
├── Windows/                    # Windows версия (WPF)
│   ├── ZapretMod.sln
│   ├── ZapretMod/             # GUI приложение
│   │   ├── Core/
│   │   │   ├── ZapretEngine.cs      # Управление winws.exe
│   │   │   └── ServiceManager.cs    # Windows Service
│   │   ├── MainWindow.xaml.cs       # Главный экран
│   │   ├── SettingsWindow.xaml.cs   # Настройки
│   │   └── DiagnosticsWindow.xaml.cs # Диагностика
│   ├── ZapretMod.Service/   # Windows Service
│   ├── general.bat          # Стратегии (как в flowseal)
│   ├── service.bat          # Управление службой
│   └── lists/               # Списки доменов
│
├── Android/                  # Android версия
│   ├── app/
│   │   └── src/main/java/com/zapretmod/app/
│   │       ├── MainActivity.kt          # Главный экран (Compose)
│   │       ├── service/
│   │       │   └── ZapretVpnService.kt  # VPN Service
│   │       ├── receiver/
│   │       │   └── BootReceiver.kt      # Auto-start
│   │       └── ui/theme/
│   │           └── Theme.kt             # Material Design
│   └── build.gradle.kts
│
├── README.md                 # Этот файл
├── LICENSE                   # MIT License
└── TECHNICAL_SPECIFICATION.md # ТЗ
```

## 🔧 Технические детали

### Как это работает

#### Windows
```
ZapretMod GUI → ZapretEngine → winws.exe (zapret) → WinDivert → Сеть
                                     ↓
                              Перехват трафика
                              DPI обход (fake packets,
                              fragmentation, TTL manipulation)
```

#### Android
```
ZapretMod UI → ZapretVpnService → Android VpnService → Сеть
                                      ↓
                               Перехват трафика
                               (аналогично flowseal,
                               но через Android VPN API)
```

### Сравнение с flowseal/zapret-discord-youtube

| Функция | flowseal | ZapretMod Windows | ZapretMod Android |
|---------|----------|-------------------|-------------------|
| GUI | Batch меню | WPF Modern UI | Jetpack Compose |
| Стратегии | ✅ 15+ | ✅ 15+ | ✅ 8+ |
| Автозапуск | Service | Service | BootReceiver |
| Логирование | Console | File + GUI | File + UI |
| Платформа | Windows | Windows 10/11 | Android 8.0+ |

## ⚙️ Требования

### Windows
- **ОС**: Windows 10 x64 (версия 1903+) или Windows 11
- **Фреймворк**: .NET 8 Desktop Runtime
- **Бинарники**: zapret-win-bundle (winws.exe, WinDivert)
- **Права**: Администратор (для службы и WinDivert)

### Android
- **ОС**: Android 8.0+ (API 26+)
- **Разрешения**: VPN, Foreground Service, Boot
- **Root**: Не требуется

## 🔍 Диагностика

### Windows
1. Откройте вкладку **"🔍 Диагностика"**
2. Проверьте:
   - Secure DNS (DoH)
   - Служба Windows
   - Бинарные файлы (winws.exe)
   - Права администратора
   - WinDivert драйвер

### Android
1. Откройте **Настройки** → **О приложении**
2. Проверьте версию и статус разрешений

## ❓ FAQ

### Q: Не работает Discord/YouTube
**A:** 
1. Попробуйте другую стратегию (FAKE TLS AUTO, SIMPLE FAKE)
2. Проверьте Secure DNS (Windows) или Private DNS (Android)
3. Обновите списки доменов в папке `lists/`

### Q: Антивирус блокирует WinDivert
**A:** Добавьте папку с приложением в исключения антивируса

### Q: Android не подключается VPN
**A:** Предоставьте разрешение при первом запуске

### Q: Как добавить свою стратегию?
**A:** 
- **Windows**: Создайте `.bat` файл по аналогии с `general.bat`
- **Android**: Добавьте стратегию в `ZapretVpnService.kt`

## 📄 Лицензия

MIT License. См. файл [LICENSE](LICENSE).

## 🤝 Вклад в проект

Приветствуются:
- Отчёты об ошибках
- Предложения по стратегиям обхода
- Pull Request'ы
- Переводы на другие языки

## ⚠️ Отказ от ответственности

Это приложение предназначено для образовательных целей и тестирования сетей.
Использование для обхода блокировок может нарушать законодательство вашей страны.
Авторы не несут ответственности за любые последствия использования данного ПО.

## 📞 Контакты

- **GitHub Issues**: [Сообщить об ошибке](https://github.com/WegVPN/zapret-mod-unoficcial/issues)
- **Оригинальный проект**: [flowseal/zapret-discord-youtube](https://github.com/flowseal/zapret-discord-youtube)
- **Zapret**: [bol-van/zapret](https://github.com/bol-van/zapret)

---

**ZapretMod** © 2024. Вдохновлено flowseal/zapret-discord-youtube. Создано с ❤️ для сообщества.
