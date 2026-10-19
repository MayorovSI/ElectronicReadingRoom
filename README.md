# Электронный читальный зал

Информационная система «Электронный читальный зал библиотеки» — MVP версия.

## Технологический стек

- **Платформа:** ASP.NET Core 8
- **Фронтенд:** Blazor WebAssembly (WASM)
- **База данных:** SQLite (Entity Framework Core)
- **Хранилище файлов:** Локальная файловая система (`./uploads`, имена — GUID)
- **Архитектура:** Трёхуровневая (UI — Business Logic — Data Access)

## Структура проекта

```
src/
├── ElectronicReadingRoom.Client/   — Blazor WASM (фронтенд)
├── ElectronicReadingRoom.Core/     — Сущности и интерфейсы
├── ElectronicReadingRoom.Data/     — DbContext, сервисы, репозитории
└── ElectronicReadingRoom.Server/   — ASP.NET Core (API, хостинг)
```

## Запуск

### Предварительные требования

- .NET 8 SDK

### Установка и запуск

```bash
dotnet restore
dotnet build

cd src/ElectronicReadingRoom.Server
dotnet run
```

Приложение будет доступно по адресу: `https://localhost:5001`

## Режимы работы

Система поддерживает два режима без аутентификации:

- **Читатель** — просмотр каталога, поиск, чтение документов
- **Библиотекарь** — управление каталогом, жанрами, CRUD документов

Режим переключается через выпадающий список в верхней панели.

## Поддерживаемые форматы документов

| Формат | Библиотека |
|--------|-----------|
| PDF | PDF.js (Mozilla) |
| EPUB | foliate-js |
| DJVU | djvujs |
| TXT | Нативное отображение |
| DOCX | docx-preview |

## API

### Документы

- `GET /api/documents` — список документов (параметры: page, pageSize, search, genreId, sortBy)
- `GET /api/documents/{id}` — получение документа
- `POST /api/documents` — создание документа (multipart/form-data)
- `PUT /api/documents/{id}` — обновление документа
- `PUT /api/documents/{id}/file` — замена файла
- `DELETE /api/documents/{id}` — удаление документа

### Авторы

- `GET /api/authors` — список всех авторов
- `GET /api/authors/search?q=` — поиск авторов
- `POST /api/authors` — создание автора

### Жанры

- `GET /api/genres` — список жанров
- `GET /api/genres/{id}` — получение жанра
- `POST /api/genres` — создание жанра
- `PUT /api/genres/{id}` — обновление жанра
- `DELETE /api/genres/{id}` — удаление жанра

### Файлы

- `GET /api/files/{path}` — получение файла
