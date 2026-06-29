# Stage 1 — build
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copiar apenas os .csproj primeiro para aproveitar o cache de restore
COPY src/TaskFlow.Domain/TaskFlow.Domain.csproj             src/TaskFlow.Domain/
COPY src/TaskFlow.Utils/TaskFlow.Utils.csproj               src/TaskFlow.Utils/
COPY src/TaskFlow.Application/TaskFlow.Application.csproj   src/TaskFlow.Application/
COPY src/TaskFlow.Infra.Persistence/TaskFlow.Infra.Persistence.csproj src/TaskFlow.Infra.Persistence/
COPY src/TaskFlow.Api/TaskFlow.Api.csproj                   src/TaskFlow.Api/

RUN dotnet restore src/TaskFlow.Api/TaskFlow.Api.csproj

# Copiar o restante e publicar
COPY src/ src/
RUN dotnet publish src/TaskFlow.Api/TaskFlow.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2 — runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# curl para o HEALTHCHECK
RUN apk add --no-cache curl

# Usuário não-root para segurança
RUN addgroup -S taskflow && adduser -S taskflow -G taskflow

COPY --from=build /app/publish .

# Diretório de dados com permissão correta
RUN mkdir -p /app/data && chown taskflow:taskflow /app/data

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:8080/projetos || exit 1

USER taskflow
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskFlow.Api.dll"]
