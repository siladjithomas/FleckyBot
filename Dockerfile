FROM mcr.microsoft.com/dotnet/sdk:6.0 AS fleckybot-build
WORKDIR /App

COPY . ./
RUN dotnet restore
RUN dotnet publish -c Debug -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS fleckybot
WORKDIR /App
COPY --from=fleckybot-build /App/out .
ENTRYPOINT ["dotnet", "Web.dll"]
