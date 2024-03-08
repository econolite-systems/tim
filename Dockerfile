FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV SolutionDir /src
WORKDIR /src
COPY . .
WORKDIR "/src/Api.Tim"
RUN dotnet build "Api.Tim.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.Tim.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.Tim.dll"]
