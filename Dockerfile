# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files for layer caching on restore
COPY *.slnx ./
COPY src/*/*.csproj ./src/
COPY tests/*/*.csproj ./tests/
RUN for file in src/*.csproj; do dir=$(basename "$file" .csproj); mkdir -p "src/$dir" && mv "$file" "src/$dir/"; done 2>/dev/null || true && \
    for file in tests/*.csproj; do dir=$(basename "$file" .csproj); mkdir -p "tests/$dir" && mv "$file" "tests/$dir/"; done 2>/dev/null || true

RUN dotnet restore

# Copy everything and publish
COPY . .
RUN dotnet publish src/Blackjack.Web/Blackjack.Web.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Blackjack.Web.dll"]
