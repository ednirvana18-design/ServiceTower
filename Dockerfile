# --- ETAPA 1: CONSTRUCCIÓN ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiamos los archivos y publicamos la app
COPY . ./
RUN dotnet publish -c Release -o out

# --- ETAPA 2: EJECUCIÓN (LINUX) ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# 1. Instalamos las librerías gráficas Y el motor wkhtmltopdf de Linux
RUN apt-get update && apt-get install -y --no-install-recommends \
    wkhtmltopdf \
    libgdiplus \
    libx11-6 \
    libc6-dev \
    libxrender1 \
    libxext6 \
    libfontconfig1 \
    libfreetype6 \
    libssl-dev \
    libpng16-16 \
    libjpeg62-turbo \
    zlib1g \
    fontconfig \
    xfonts-75dpi \
    xfonts-base \
    && rm -rf /var/lib/apt/lists/*

# 2. Copiamos los archivos publicados desde la etapa de build
COPY --from=build /app/out .

# 3. Permisos de ejecución (por si acaso)
RUN chmod +x /usr/bin/wkhtmltopdf

# 4. Arrancamos la aplicación
ENTRYPOINT ["dotnet", "ServiceTowerWeb.dll"]