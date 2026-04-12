FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BookGuide.API/BookGuide.API.csproj", "BookGuide.API/"]
RUN dotnet restore "BookGuide.API/BookGuide.API.csproj"

COPY . .
WORKDIR /src/BookGuide.API
RUN dotnet publish "BookGuide.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookGuide.API.dll"]