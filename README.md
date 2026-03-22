# 🛡 ZapretMod - DPI Bypass для Discord, YouTube, Telegram

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Windows](https://img.shields.io/badge/platform-Windows%2010%2F11%20x64-lightgrey)]()
[![Android](https://img.shields.io/badge/platform-Android%208.0+-brightgreen)]()
[![Inspired by](https://img.shields.io/badge/inspired%20by-flowseal%2Fzapret--discord--youtube-orange)](https://github.com/flowseal/zapret-discord-youtube)

**ZapretMod** — это кроссплатформенное приложение с **профессиональным GUI** для обхода DPI-блокировок (Deep Packet Inspection).

Вдохновлено проектом [flowseal/zapret-discord-youtube](https://github.com/flowseal/zapret-discord-youtube).

## 📥 Скачать

### Windows (готовая версия)
🔗 **[Скачать ZIP (612 KB)](https://github.com/WegVPN/zapret-mod-unoficcial/releases/download/v2.0.0/ZapretMod-v2.0.0-win-x64.zip)**

**Что в архиве:**
- ✅ ZapretMod.exe - приложение с красивым GUI
- ✅ download-binaries.bat - автозагрузка required файлов
- ✅ Batch-файлы стратегий (как в flowseal)
- ✅ Списки доменов

### Android (в разработке)
Скоро...

## 🚀 Быстрый старт (Windows)

### Шаг 1: Распакуйте
Распакуйте ZIP архив в любую папку.

### Шаг 2: Скачайте бинарные файлы
**Вариант A (Автоматически):**
1. Запустите `download-binaries.bat`
2. Дождитесь окончания загрузки

**Вариант B (Вручную):**
1. Скачайте с https://github.com/bol-van/zapret-win-bundle/releases/latest
2. Скопируйте файлы в папку `bin\`:
   - `winws.exe`
   - `WinDivert64.sys`
   - `WinDivert64.dll`

### Шаг 3: Запустите
1. Запустите `ZapretMod.exe` **от имени администратора**
2. Выберите стратегию
3. Нажмите **"▶ ЗАПУСТИТЬ"**

## ✨ Возможности

### Windows версия
- 🎨 **Современный GUI** - профессиональный дизайн в тёмной теме
- 📋 **8+ стратегий** - готовые настройки для Discord, YouTube, Telegram
- 🚀 **Автозапуск** - установка службы Windows
- 🎮 **Game Filter** - исключение игр из обработки
- 🔍 **Диагностика** - проверка всех компонентов
- 📜 **Логи** - подробное логирование
- 📊 **Системный трей** - работа в фоне

### Стратегии (как в flowseal)
| Стратегия | Описание |
|-----------|----------|
| **Discord + YouTube + Telegram** | Основная для всех сервисов |
| **Discord Only** | Только Discord |
| **YouTube Only** | Только YouTube |
| **Telegram Only** | Только Telegram |
| **FAKE TLS AUTO** | Автоматический fake TLS |
| **SIMPLE FAKE** | Простой fake метод |

## 📸 Скриншоты

### Главный экран
```
┌─────────────────────────────────────────────────────────┐
│  🛡 ZapretMod                          ● Остановлено    │
│     DPI Bypass for Discord, YouTube, Telegram           │
├─────────────────────────────────────────────────────────┤
│  📋 Стратегия обхода                                    │
│  [Discord + YouTube + Telegram        ▼]               │
│  Основной профиль для Discord, YouTube и Telegram       │
├─────────────────────────────────────────────────────────┤
│  🚀 Автозапуск  🎮 Game Filter  🔍 Диагностика         │
├─────────────────────────────────────────────────────────┤
│  📜 Логи                           [🗑 Очистить] [💾 Сохранить]
│  ┌─────────────────────────────────────────────────────┐│
│  │ [07:03:15] === ZapretMod запущен ===               ││
│  │ [07:03:20] ✓ winws.exe найден                      ││
│  │ [07:03:25] ✓ Стратегия запущена                    ││
│  └─────────────────────────────────────────────────────┘│
├─────────────────────────────────────────────────────────┤
│  ⚙ Настройки  🔧 Служба           [▶ ЗАПУСТИТЬ]        │
└─────────────────────────────────────────────────────────┘
```

## ⚙️ Требования

### Windows
- **ОС**: Windows 10/11 x64
- **Фреймворк**: .NET 8 Desktop Runtime
- **Бинарники**: zapret-win-bundle (скачиваются автоматически)
- **Права**: Администратор

## 🔍 Диагностика

Если не работает:

1. **Проверьте файлы**: Откройте "🔍 Диагностика" → проверьте наличие файлов
2. **Запуск от админа**: Убедитесь что запущены от администратора
3. **Secure DNS**: Включите Secure DNS в диагностике
4. **Антивирус**: Добавьте папку в исключения

## 📁 Структура проекта

```
zapret-mod-unoficcial/
├── Windows/
│   ├── ZapretMod/
│   │   ├── Core/
│   │   │   ├── ZapretEngine.cs      # Управление winws.exe
│   │   │   └── ServiceManager.cs    # Windows Service
│   │   ├── App.xaml                 # Ресурсы и стили
│   │   ├── MainWindow.xaml          # Главный экран (XAML)
│   │   ├── SettingsWindow.xaml.cs   # Настройки
│   │   └── DiagnosticsWindow.xaml.cs # Диагностика
│   ├── download-binaries.bat        # Автозагрузка файлов
│   ├── general.bat                  # Стратегии
│   ├── service.bat                  # Управление службой
│   └── lists/                       # Списки доменов
│
├── Android/                         # Android версия (в разработке)
│   └── app/
│
└── README.md                        # Этот файл
```

## 🔧 Как это работает

```
ZapretMod GUI → ZapretEngine → winws.exe (zapret) → WinDivert → Сеть
                                     ↓
                              Перехват трафика
                              DPI обход (fake packets,
                              fragmentation, TTL manipulation)
```

## ❓ FAQ

### Q: Не работает Discord/YouTube
**A:** 
1. Попробуйте другую стратегию (FAKE TLS AUTO, SIMPLE FAKE)
2. Проверьте Secure DNS
3. Обновите списки доменов

### Q: Антивирус блокирует WinDivert
**A:** Добавьте папку с приложением в исключения антивируса

### Q: Как добавить свою стратегию?
**A:** Создайте `.bat` файл по аналогии с `general.bat`

### Q: Автоматическое скачивание не работает
**A:** Запустите `download-binaries.bat` или скачайте вручную

## 📄 Лицензия

MIT License. См. файл [LICENSE](LICENSE).

## 🤝 Вклад в проект

Приветствуются:
- Отчёты об ошибках
- Предложения по стратегиям
- Pull Request'ы

## ⚠️ Отказ от ответственности

Это приложение предназначено для образовательных целей и тестирования сетей.
Использование для обхода блокировок может нарушать законодательство вашей страны.

## 📞 Контакты

- **GitHub**: https://github.com/WegVPN/zapret-mod-unoficcial
- **Оригинал**: https://github.com/flowseal/zapret-discord-youtube
- **Zapret**: https://github.com/bol-van/zapret

---

**ZapretMod v2.0.0** © 2024. Вдохновлено flowseal/zapret-discord-youtube.
