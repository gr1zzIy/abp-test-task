FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Спочатку копіюємо project-файли окремо,
# щоб Docker міг кешувати restore залежностей.
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/WebApi/WebApi.csproj src/WebApi/

RUN dotnet restore src/WebApi/WebApi.csproj

COPY src/ src/

RUN dotnet publish src/WebApi/WebApi.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WebApi.dll"]