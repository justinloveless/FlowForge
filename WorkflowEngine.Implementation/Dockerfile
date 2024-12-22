FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WorkflowEngine.Implementation/WorkflowEngine.Implementation.csproj", "WorkflowEngine.Implementation/"]
RUN dotnet restore "WorkflowEngine.Implementation/WorkflowEngine.Implementation.csproj"
COPY . .
WORKDIR "/src/WorkflowEngine.Implementation"
RUN dotnet build "WorkflowEngine.Implementation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WorkflowEngine.Implementation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkflowEngine.Implementation.dll"]
