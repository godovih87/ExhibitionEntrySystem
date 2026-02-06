## 🚗 ExhibitionEntrySystem — система регистрации пропусков  

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET-Core-512BD4?style=for-the-badge&logo=dotnet)
![MVC](https://img.shields.io/badge/Architecture-MVC-0A66C2?style=for-the-badge)
![MSSQL](https://img.shields.io/badge/MS_SQL-Server-CC2927?style=for-the-badge&logo=microsoft-sql-server)
![HTML](https://img.shields.io/badge/HTML-5-E34F26?style=for-the-badge&logo=html5)
![CSS](https://img.shields.io/badge/CSS-3-1572B6?style=for-the-badge&logo=css3)
![JavaScript](https://img.shields.io/badge/JavaScript-ES6-F7DF1E?style=for-the-badge&logo=javascript)
![Entity Framework](https://img.shields.io/badge/Entity_Framework-Core-512BD4?style=for-the-badge)

**ExhibitionEntrySystem** — веб-приложение для автоматизации пропускного режима на территории предприятия.  
Работает через браузер и не требует установки дополнительного программного обеспечения.
---

### 🚀 Основные возможности  

## 🔐 Регистрация пропусков  
Создание разового пропуска  
Ввод данных посетителя и транспортного средства  
Выбор павильона и времени въезда и выезда
---

## 📱 QR-коды  
Автоматическая генерация QR-кода  
Использование QR-кода для идентификации пропуска  
Сканирование без ручного ввода данных 
---

## 🚦 Учет въезда и выезда  
Регистрация въезда по QR-коду  
Регистрация выезда по QR-коду  
Автоматическая фиксация времени  
---

## 🛠 Технологический стек  

### Backend  
- ASP.NET Core MVC (.NET 8.0)  
- MS SQL Server  
- Entity Framework Core (Code First)  
- QR-code API    

### Frontend   
- HTML / CSS / JavaScript  

---

## 📥 Установка и запуск

### ▶ Локальный запуск (Visual Studio)

#### 1. Настройка базы данных

Файл `ExhibitionEntrySystem/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ExhibitionEntrySystemDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Применение миграций:

```powershell
Update-Database
```

---

#### 2. Запуск сервера

- Установить проект `ExhibitionEntrySystem` стартовым
- Запустить (`F5`)
- Откроется по адресу:

```
https://localhost:7295/
```
