FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ECommerceRanking.csproj", "./"]
RUN dotnet restore "ECommerceRanking.csproj"
COPY . .
RUN dotnet build "ECommerceRanking.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ECommerceRanking.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECommerceRanking.dll"]
