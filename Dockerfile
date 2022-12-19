FROM mcr.microsoft.com/dotnet/sdk:6.0 AS fleckybot-base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS fleckybot-build
COPY . /src
WORKDIR /src
RUN ls
RUN dotnet build src/Bot/Bot.csproj -c Release -o /app/build

FROM fleckybot-build AS fleckybot-publish
RUN dotnet publish src/Bot/Bot.csproj -c Release -o /app/publish

FROM fleckybot-base as fleckybot
WORKDIR /app
COPY --from=fleckybot-publish /app/publish .
ENTRYPOINT ["dotnet", "Web.dll"]