#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Ferrite/Ferrite.csproj", "Ferrite/"]
COPY ["Ferrite.Crypto/Ferrite.Crypto.csproj", "Ferrite.Crypto/"]
COPY ["Ferrite.Utils/Ferrite.Utils.csproj", "Ferrite.Utils/"]
COPY ["Ferrite.Transport/Ferrite.Transport.csproj", "Ferrite.Transport/"]
COPY ["Ferrite.TL/Ferrite.TL.csproj", "Ferrite.TL/"]
COPY ["Ferrite.Data/Ferrite.Data.csproj", "Ferrite.Data/"]
COPY ["Ferrite.Core/Ferrite.Core.csproj", "Ferrite.Core/"]
RUN dotnet restore "Ferrite/Ferrite.csproj"
COPY . .
WORKDIR "/src/Ferrite"
RUN dotnet build "Ferrite.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ferrite.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ferrite.dll"]
