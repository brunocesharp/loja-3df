FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY AssistenteDB.sln .
COPY src/Domain/AssistenteDB.Domain.csproj src/Domain/
COPY src/Application/AssistenteDB.Application.csproj src/Application/
COPY src/Data/AssistenteDB.Data.csproj src/Data/
COPY src/Presentation/AssistenteDB.Mcp/AssistenteDB.Mcp.csproj src/Presentation/AssistenteDB.Mcp/

RUN dotnet restore src/Presentation/AssistenteDB.Mcp/AssistenteDB.Mcp.csproj

COPY src/Domain/ src/Domain/
COPY src/Application/ src/Application/
COPY src/Data/ src/Data/
COPY src/Presentation/AssistenteDB.Mcp/ src/Presentation/AssistenteDB.Mcp/

RUN dotnet publish src/Presentation/AssistenteDB.Mcp/AssistenteDB.Mcp.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AssistenteDB.Mcp.dll"]
