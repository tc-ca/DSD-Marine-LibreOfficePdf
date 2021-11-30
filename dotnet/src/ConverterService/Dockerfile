#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM --platform=linux/amd64 ubuntu:20.04 AS base
# LibreOffice and other PDF tools
RUN apt-get update -y
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get install libreoffice -y -q
RUN apt-get install ghostscript -y; apt-get install pdftk-java -y
# ASP.NET Core 6.0 runtime
RUN apt-get install wget
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN apt-get install -y apt-transport-https && apt-get update && apt-get install -y aspnetcore-runtime-6.0

ENV DOTNET_VERSION=6.0.0 ASPNETCORE_URLS=http://+:80 DOTNET_RUNNING_IN_CONTAINER=true ASPNETCORE_ENVIRONMENT=Development
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/ConverterService/ConverterService.csproj", "src/ConverterService/"]
RUN dotnet restore "src/ConverterService/ConverterService.csproj"
COPY . .
WORKDIR "/src/src/ConverterService"
RUN dotnet build "ConverterService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConverterService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConverterService.dll"]