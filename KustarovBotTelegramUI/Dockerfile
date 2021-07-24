FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine
COPY ../build/KustarovBotTelegramUI App/
WORKDIR /App
ENTRYPOINT ["dotnet", "KustarovBotTelegramUI.dll"]