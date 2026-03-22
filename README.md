# 🛡 ZapretMod - DPI Bypass для Discord, YouTube, Telegram

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Version](https://img.shields.io/github/v/release/WegVPN/zapret-mod-unoficcial?color=5865F2)]()
[![Downloads](https://img.shields.io/github/downloads/WegVPN/zapret-mod-unoficcial/total)]()

**ZapretMod** — это современное приложение с **профессиональным GUI** для обхода DPI-блокировок.

Вдохновлено проектом [flowseal/zapret-discord-youtube](https://github.com/flowseal/zapret-discord-youtube).

## 📥 Скачать

### Windows (готовая версия)

**[📥 Скачать ZIP (614 KB)](https://github.com/WegVPN/zapret-mod-unoficcial/releases/download/v2.0.0/ZapretMod-v2.0.0-win-x64.zip)**

## 🚀 Быстрый старт

### 1️⃣ Распакуйте
Распакуйте ZIP архив в любую папку

### 2️⃣ Скачайте бинарные файлы
**Автоматически (в приложении):**
- Запустите `ZapretMod.exe` от имени администратора
- Нажмите кнопку **"📥 Скачать файлы"**

**Или через batch-файл:**
- Запустите `download-binaries.bat`

**Или вручную:**
- Скачайте с https://github.com/bol-van/zapret-win-bundle/releases/latest
- Скопируйте в папку `bin\`:
  - `winws.exe`
  - `WinDivert64.sys`
  - `WinDivert64.dll`

### 3️⃣ Запустите защиту
1. Запустите `ZapretMod.exe` **от имени администратора**
2. Выберите стратегию
3. Нажмите **"▶ ЗАПУСТИТЬ"**

## ✨ Возможности

- 🎨 **Современный GUI** - Discord-подобный дизайн (#5865F2)
- 🌑 **Тёмная тема** - приятные цвета для глаз
- 📋 **6 стратегий** - Discord, YouTube, Telegram, FAKE TLS, SIMPLE FAKE
- 📥 **Автозагрузка** - скачивание файлов из приложения
- 🚀 **Автозапуск** - установка службы Windows
- 🔍 **Диагностика** - проверка всех компонентов
- 📜 **Логи** - цветное логирование в реальном времени
- 📊 **Системный трей** - работа в фоне
- 🎮 **Game Filter** - исключение игр

## 📋 Стратегии

| Стратегия | Описание | Для чего |
|-----------|----------|----------|
| **Discord + YouTube + Telegram** | Основная для всех сервисов | Рекомендуется |
| **Discord Only** | Только Discord | Голосовые каналы |
| **YouTube Only** | Только YouTube | 4K стриминг |
| **Telegram Only** | Только Telegram | Мессенджер + proxy |
| **FAKE TLS AUTO** | Автоматический fake TLS | Универсальная |
| **SIMPLE FAKE** | Простой fake метод | Минимальные задержки |

## 🖼 Интерфейс

```
┌─────────────────────────────────────────────────────────────────────────┐
│  ┌───┐  ZapretMod                                    ● Остановлено     │
│  │🛡 │  DPI Bypass для Discord, YouTube, Telegram                       │
│  └───┘                                                                  │
├─────────────────────────────────────────────────────────────────────────┤
│  📋 Стратегия обхода                                                    │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │ Discord + YouTube + Telegram                                   ▼│   │
│  └─────────────────────────────────────────────────────────────────┘   │
│  💡 Основной профиль для всех сервисов. Рекомендуется...                │
├─────────────────────────────────────────────────────────────────────────┤
│  ⚙ Опции                        🔧 Действия                             │
│  ☐ Автозапуск с Windows         🔍 Диагностика                         │
│  ☐ Game Filter (UDP >1023)      🔧 Управление службой                 │
│  ☑ Сворачивать в трей           📥 Скачать файлы                       │
├─────────────────────────────────────────────────────────────────────────┤
│  📜 Логи работы                              [🗑 Очистить] [💾 Сохранить]
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │ [07:25:00] ═══ ZapretMod запущен ═══                             │  │
│  │ [07:25:05] ✓ Все файлы найдены                                   │  │
│  │ [07:25:10] ✓ Стратегия 'Discord + YouTube...' запущена           │  │
│  │ [07:25:11] winws started with PID: 12345                         │  │
│  └───────────────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────────────────┤
│  ⚙ Настройки                               [▶ ЗАПУСТИТЬ]               │
└─────────────────────────────────────────────────────────────────────────┘
```

## ⚙ Требования

- **ОС**: Windows 10/11 x64
- **Фреймворк**: .NET 8 Desktop Runtime
- **Бинарники**: zapret-win-bundle (скачиваются автоматически)
- **Права**: Администратор

## 🔍 Диагностика

Если не работает:

1. **Проверьте файлы**: Откройте "🔍 Диагностика"
2. **Запуск от админа**: Убедитесь что запущены от администратора
3. **Secure DNS**: Включите Secure DNS в диагностике
4. **Антивирус**: Добавьте папку в исключения

## ❓ FAQ

### Q: Не работает Discord/YouTube
**A:** Попробуйте другую стратегию (FAKE TLS AUTO, SIMPLE FAKE)

### Q: Антивирус блокирует WinDivert
**A:** Добавьте папку с приложением в исключения антивируса

### Q: Не скачиваются файлы
**A:** Запустите `download-binaries.bat` или скачайте вручную

### Q: Как добавить в автозагрузку?
**A:** Включите опцию "Автозапуск с Windows" или установите службу

## 📁 Структура проекта

```
ZapretMod/
├── Core/
│   ├── ZapretEngine.cs      # Управление winws.exe
│   └── ServiceManager.cs    # Windows Service
├── App.xaml                 # Ресурсы и стили (Discord colors)
├── MainWindow.xaml          # Главный экран
├── SettingsWindow.xaml.cs   # Настройки
├── DiagnosticsWindow.xaml.cs # Диагностика
├── download-binaries.bat    # Автозагрузка файлов
├── service.bat              # Управление службой
├── lists/
│   ├── discord.txt
│   ├── youtube.txt
│   └── telegram.txt
└── README.txt               # Инструкция
```

## 🔧 Как это работает

```
ZapretMod GUI → ZapretEngine → winws.exe (zapret) → WinDivert → Сеть
                                     ↓
                              Перехват трафика
                              DPI обход (fake packets,
                              fragmentation, TTL manipulation)
```

## 📄 Лицензия

MIT License.

## 📞 Ссылки

- **GitHub**: https://github.com/WegVPN/zapret-mod-unoficcial
- **zapret-win-bundle**: https://github.com/bol-van/zapret-win-bundle
- **Оригинал**: https://github.com/flowseal/zapret-discord-youtube

---

**ZapretMod v2.0.0** © 2024. Вдохновлено flowseal/zapret-discord-youtube.
