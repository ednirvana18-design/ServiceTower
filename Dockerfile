# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . ./
RUN dotnet publish -c Release -o out

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# --- INICIO DE CORRECCIÓN PARA PDF (Rotativa/wkhtmltopdf) ---
# Instalamos las librerías gráficas que Linux necesita para "dibujar" el PDF
RUN apt-get update && apt-get install -y --no-install-recommends \
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
# --- FIN DE CORRECCIÓN ---

COPY --from=build /app/out .

# Asegúrate de que el nombre del .dll sea exactamente este
ENTRYPOINT ["dotnet", "ServiceTowerWeb.dll"]