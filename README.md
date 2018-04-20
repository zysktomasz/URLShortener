URL Shortening web app z użyciem ASP .NET Core 2.0 MVC + EF Core

Prezentacja:

![skracanie](https://i.imgur.com/DfUYxlE.gif)

Rejestracja
![rejestracja](https://i.imgur.com/wmVP7VA.gif)

Logowanie
![logowanie](https://i.imgur.com/5VoofXU.gif)

Lista linkow
![lista linkow](https://i.imgur.com/bC8lq8t.png)

# Changelog

## [0.0.3.3] - 2018-04-20
### Dodane
- Usuwanie kont uzytkownikow (wraz z usunieciem konta, usuwane sa wszystkie linki uzytkownika)
- Lista wszystkich linkow w PA (/Admin/Links/ListLinks)
- Usuwanie linkow z PA
- Navbar - dodany panel admina widoczny tylko dla admina

### Zmienione
- teraz poprawione, unikalne nazwy metod w AdminController (czytelniejszy routing)

## [0.0.3.2] - 2018-04-19
### Dodane
- Panel Administratora - chroniony Policy (wymagajacy Roli Administratora) (/Admin)
- Lista Uzytkownikow w PA (/Admin/Users/ListUsers)
- 2 nowe ViewModele to powyzszych operacji (UserListVM, UserDeleteVM)

## [0.0.3.1] - 2018-04-19
### Dodane
- Paginacja linkow

### Zmienione
- Poprawione przekierowania na liscie linkow (z /account/edit?id=2 na /account/edit/2)
- Zaktualizowany navbar

## [0.0.3.0] - 2018-04-18
### Dodane
- Edycja wlasnych linkow (/Account/Link)
- Usuwanie linkow (/Account/Delete)
- Nowy UrlEditViewModel

### Zmienione
- Wyciagniecie metody tworzacej losowa nazwe linku do klasy statycznej (dostepne dla dwoch kontrolerow)

## [0.0.2.0] - 2018-04-14
### Dodane
- wlasna implementacja IdentityUser
- polaczenie skracanych linkow z uzytkownikami
- lista linkow dla danego uzytkownika

### Zmienione
- Jedna baza danych i jeden DbContext dla Indetity i pozostalych tabel (zamiast dwoch oddzielnych) 

## [0.0.1.0] - 2018-04-12
### Dodane
- Mozliwosc tworzenia konta i logowania

## [0.0.0.2] - 2018-04-11
### Dodane
- Opcja podania wlasnej nazwy skroconego linka
- Dodane atrybuty weryfikujace poprawnosc pol

### Zmienione
- Model obslugujacy widok strony glownej (z Url na UrlViewModel)