# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Use the .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy everything and build
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Run the app
ENTRYPOINT ["dotnet", "DailyStatusApp.dll"]
