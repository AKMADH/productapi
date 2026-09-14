FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["SecureProductApi.csproj", "./"]

RUN dotnet restore "SecureProductApi.csproj"

COPY . .

RUN dotnet publish "SecureProductApi.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "SecureProductApi.dll"]