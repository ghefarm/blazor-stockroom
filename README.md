# Stockroom

The companion code of the book *Blazor in .NET 10: From First Page to Shipped App*.

Stockroom is a stock and order manager for a small warehouse. The book builds it chapter by chapter, with Blazor on
.NET 10, EF Core with SQLite and ASP.NET Core Identity. All names and numbers in it are invented.

## One tag per chapter

Every chapter that adds code ends in one commit with a tag: `ch02` to `ch30`. Chapters 1, 28, 31 and 32 explain and
add no code, so they have no tag. The tag `ch09` is the state of Stockroom at the end of chapter 9.

```text
git clone https://github.com/ghefarm/blazor-stockroom.git
cd blazor-stockroom
git checkout ch09
```

Git then says that you are in a "detached HEAD" state. That is normal for a tag.

## Build, test, run

You need the .NET 10 SDK. From the folder of the repository:

```text
dotnet tool restore
dotnet build Stockroom.slnx
dotnet test Stockroom.slnx
dotnet run --project src/Stockroom.Web
```

`dotnet tool restore` installs the local `dotnet-ef` tool from chapter 9. Every tag builds without warnings, and from
chapter 9 on its tests pass.

From chapter 12 on, Stockroom needs a sign-in. In the `Development` environment, the first administrator is `admin`
with the password from `src/Stockroom.Web/appsettings.Development.json`. Stockroom asks you to change it at the first
sign-in. Up to chapter 29, the database is created in the `App_Data` folder of the web project. From chapter 30 on, it
is in the data folder, which is `C:\ProgramData\Stockroom` on Windows. The setting `Stockroom:DataRoot` chooses another
folder.

The question box of chapter 27 is off by default. It needs an OpenAI-compatible endpoint and the setting
`Questions:Enabled`.

## Layout

| Folder | What |
|---|---|
| `src/Stockroom.Core` | data model, EF Core, services, business rules |
| `src/Stockroom.Web` | the Blazor Web App |
| `src/Stockroom.Questions` | the core of the AI question (chapter 27) |
| `tests/` | NUnit tests with NSubstitute and bUnit |
| `samples/` | small samples that the book shows |
| `deploy/` | scripts for the installation on Windows (chapter 30) |
| `tools/` | helpers of the installer in chapter 30: folder rights, the program folder swap, a syntax check for the scripts |

## License

The code is licensed under the MIT License, see `LICENSE`. The text of the book is not part of this repository.
