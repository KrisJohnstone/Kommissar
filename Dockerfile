﻿FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Kommissar/Kommissar.csproj", "Kommissar/"]
RUN dotnet restore "Kommissar/Kommissar.csproj"
WORKDIR "/src/Kommissar"
COPY . .
RUN dotnet build "Kommissar.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kommissar.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kommissar.dll"]
