
Podstawowa funkcjonalność tej aplikacji to skracanie długich linków na krótsze, o losowej lub wybranej nazwie (np. `example.com/nazwa`). Dodatkowo, z wykorzystaniem EF Core, został zaimplementowany system autentykacji i autoryzacji. Autentykacja wymaga logowania za pośrednictwem emaila i hasła. Autoryzacja umożliwia użytkownikom:

- **podstawowym**: dostęp do listy utworzonych przez siebie linków, ich edycji jak i usuwania
- **administratorom**: dostęp do dodatkowego panelu kontrolnego, z funkcjonalnościami zarządzania użytkownikami, wszystkimi linkami, jak i blokowaniem domen(linków).

![alt text](https://i.imgur.com/XodcWLu.png)

----------


### Wykorzystane technologie
- ASP .NET Core 2.0 (wzorzec projektowy MVC)
- Razor Views + Bootstrap (prosty front aplikacji)
- npm (manager pakietów jquery,bootstrap)
- Entity Framework Core (Code First, obsługa baz danych MSSQL)
- ASP .NET Core Identity (autoryzacja/autentykacja)


----------

----------


### Struktura solucji projektu
Podstawowa struktura używana przy tworzeniu pustego projektu ASP .NET Core MVC, rozbudowana o dodatkowe foldery:

- Data - zawiera `ApplicationDbContext`, dziedziczący po `IdentityDbContext`, rozbudowany o dodatkowe modele (*Url, BlockedDomain*)
- Helper - ze statyczną klasą Helper o pomocnicznych metodach (`GenerateRandomUrlName()`, `GetUrlDomain(string url)`)
- Models/ViewModels - modele pod widoki 

![Struktura projektu](https://i.imgur.com/YuuiLSY.png)



----------

----------


### Routing
Kolejność mapowania requestów do kontrolerów w **`Startup.cs`**

- 1: strona główna, dostępna jedynie pod adresem `/Index` lub `/`
- 2: metoda przekierowująca pod adres przypisany argumentowi w linku, np dla linku: `example.com/Dh2x41Jl` uruchamiana metoda `HomeController.RedirectToTarget()` sprawdza czy istnieje w bazie link powiązany z nazwą "**Dh2x41Jl**". Jeśli tak, i domena ta nie jest zablokowana - następuje przekierowanie, jeśli nie - przenosi użytkownika do strony głównej

```csharp
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "",
                    template: "{action=Index}",
                    defaults: new { controller = "Home" });

                routes.MapRoute(
                    name: "targetRedirection",
                    template: "{urlName}",
                    defaults: new { controller = "Home", action = "RedirectToTarget" });
            });
```

Dla `AccountController.cs` używany jest atrybut `[Route]`
```csharp
[Route("[controller]/[action]/{id?}")]
    public class AccountController : Controller
    { ... }
```

**Route Attribute** jest również używany dla poszczególnych metod w `AdminController.cs`, w celu zastosowania stałych nazw w linku, jak np:
```csharp
[Route("[controller]/Links/[action]")]
        public async Task<IActionResult> BlockedDomains(int? page)
        { ... }
```
umożliwiające adres: `example.com/Admin/Links/BlockedDomains`


----------

----------


### Panel użytkownika
Zwykły użytkownik posiada dostęp do listy własnych linków, ich edycji (zmiana nazwy) oraz usuwania (przy usuwaniu dodatkowo jest zabezpieczenie wymagające ponownego potwierdzenia chęci usunięcia).

<div class="alert alert-info" role="alert">
  Do tej listy, jak i list w panelu administratora zaimplementowana jest prosta  <a href="https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page?view=aspnetcore-2.1" target="_blank"><strong>paginacja</strong> z dokumentacji microsoftu.</a>
</div>

![Panel użytkownika - lista linków](https://i.imgur.com/AhMu3Iy.png)

##### Kod z `AccountController.cs` *(lista linków użytkownika)*
```csharp
// GET: Account/List
        public async Task<IActionResult> List(int? page)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            int pageSize = 6;
            var urls = await PaginatedList<Url>.CreateAsync(
                _context.Urls
                .Where(u => u.User == currentUser)
                .OrderByDescending(u => u.UrlId), page ?? 1, pageSize);

            return View(urls);
        }
```

----------

![Panel użytkownika - edycja linku](https://i.imgur.com/MLupY1a.png)

![Panel użytkownika - potwierdzenie usunięcia linku](https://i.imgur.com/ref9wI9.png)


----------

----------



### Panel Administratora
Autoryzacja na podstawie Roli (*"Administrator"*) użytkownika, z wykorzystaniem **Authorize Attribute** o wbudowanym parametrze **Roles**

W `AdminController.cs`

```csharp
[Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    { ... }
```

**Dostępne funkcje:**

- usuwanie użytkowników, (po kliknięciu "Delete" jest dodatkowe potwierdzenie polecenia)
- wszystkie linki użytkowników (ich usuwanie, i blokowanie domeny z pozycji listy)
- zarządzanie blokowanymi domenami (blokowanie/odblokowanie)

> **blokada domeny** - oznacza, ze nie mozna skrócić linku z danej domeny, oraz aktualne linki przekierowywujące do domen zablokowanych nie działają.
> **usuwanie konta użytkownika** - wraz z usuwaniem użytkownika - usuwane są też wszystkie jego linki 

![Panel administratora - lista użytkowników](https://i.imgur.com/EVPVWqG.png)

![Panel administratora - potwierdzenie usunięcia użytkownika](https://i.imgur.com/Kds6PIO.png)

![Panel administratora - lista linków](https://i.imgur.com/mPq9byO.png)

##### Kod z `/Views/Admin/Links/List.cshtml' *(dla listy wszystkich linkow)*

```html
@model PaginatedList<UrlViewViewModel>
@{
    ViewData["Title"] = "List";
}

<div style="max-width: 1100px; margin-left: auto; margin-right: auto;">
    <h2>List of all links</h2>
    <h3 class="text-danger text-center">@TempData["error"]</h3>
    <div class="table-responsive">
        <table class="table table-hover table-bordered table-sm">
            <thead class="thead-dark">
                <tr>
                    <th>
                        User
                    </th>
                    <th>
                        Name
                    </th>
                    <th>
                        Target Url
                    </th>
                    <th></th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                @foreach (var item in Model)
                {
                var blockedDomainInfo = item.IsBlocked ? "table-danger" : "";
                <tr class="@blockedDomainInfo">
                    <td>
                        @Html.DisplayFor(modelItem => item.User.UserName)
                    </td>
                    <td>
                        @Html.DisplayFor(modelItem => item.Name)
                    </td>
                    <td>
                        @Html.DisplayFor(modelItem => item.TargetUrl)
                    </td>
                    <td>
                        <form asp-action="DeleteLink" asp-route-id="@item.UrlId" method="post">
                            <button type="submit" class="btn btn-danger btn-sm">Delete</button>
                        </form>
                    </td>
                    <td>
                        @if (item.IsBlocked)
                        {
                        <form asp-action="UnblockDomain" method="post">
                            <input type="hidden" asp-for="@item.TargetUrl" />
                            <button type="submit" class="btn btn-secondary btn-sm">Unblock Domain</button>
                        </form>
                        }
                        else {
                        <form asp-action="BlockDomain" method="post">
                            <input type="hidden" asp-for="@item.TargetUrl" />
                            <button type="submit" class="btn btn-warning btn-sm">Block Domain</button>
                        </form>
                        }
                    </td>
                </tr>
        }
            </tbody>
        </table>
        @{
        var prevDisabled = !Model.HasPreviousPage ? "disabled" : "";
        var nextDisabled = !Model.HasNextPage ? "disabled" : "";
        }

        <a asp-action="ListLinks"
           asp-route-page="@(Model.PageIndex - 1)"
           class="btn btn-dark @prevDisabled">
            Previous
        </a>
        <a asp-action="ListLinks"
           asp-route-page="@(Model.PageIndex + 1)"
           class="btn btn-dark @nextDisabled">
            Next
        </a>
    </div>
</div>
```
![Panel administratora - blokada domen](https://i.imgur.com/PhRyXMw.png)

# Changelog

## [0.0.4.1] - 2018-04-22
### Dodane
- Przycisk kopiujacy skrocony link do schowka (js)
- Podglad adresu docelowego przed przekierowaniem (/Preview/NAZWA_SKROCONEGO_ADRESU)

### Zmienione


## [0.0.4.0] - 2018-04-21
### Dodane
- Blokowanie/Odblokowanie domen z poziomu Listy Linkow (w PA)
- Utworzenie UrlViewViewModel (zawiera wlasciwosci modelu Url i wlasciwosc IsBlocked)
- Paginacja listy linkow w PA
- Walidacja skracanego linku z lista zablokowanych domen na stronie glownej
- Walidacja z lista zablokowanych domen podczas przekierowywania pod skrocony adres
- Lista zablokowanych domen

### Zmienione
- Zmiana aktualnego UrlViewModel na UrlCreateViewModel 

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
