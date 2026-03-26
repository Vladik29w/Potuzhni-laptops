FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["LaptopServer.csproj", "./"]
RUN dotnet restore "./LaptopServer.csproj"

COPY . .
RUN dotnet publish "LaptopServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "LaptopServer.dll"]