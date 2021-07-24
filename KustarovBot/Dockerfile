FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine
COPY ../build/KustarovBot App/
WORKDIR /App
ENTRYPOINT ["dotnet", "KustarovBot.dll"]