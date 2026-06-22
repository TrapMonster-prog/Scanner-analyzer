# Scanner Analyzer

**Scanner Analyzer** — это консольный инструмент для автоматизированного локального аудита безопасности операционной системы **Windows**.

---

## Установка и запуск

### Требования
- Runtime .NET 10+
- *Visual Studio 2026* / *Rider 2026.1* / *Visual Studio Code* (с установленными зависимостями)

---

### Сборка и запуск

1. Клонирование репозитория:
```
git clone https://github.com/TrapMonster-prog/Scanner-analyzer.git
cd Scanner-analyzer
```
2. Открытие решения (*Scanner-analyzer.sln* или *Scanner-analyzer.csproj*)

3. Сборка проекта:
В Visual Studio вкладка *Собрать*, далее в выпадающем контексном меню *Собрать решение* или через командную строку / терминал / PowerShell:
```
dotnet build
```
4. Запуск сканера:
```
dotnet run
```
