FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Ecommerce.sln .
COPY Ecommerce.WebApi/Ecommerce.WebApi.csproj Ecommerce.WebApi/
COPY Ecommerce.Application/Ecommerce.Application.csproj Ecommerce.Application/
COPY Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj Ecommerce.Infrastructure/
COPY Ecommerce.Domain/Ecommerce.Domain.csproj Ecommerce.Domain/
RUN dotnet restore Ecommerce.sln

COPY . .
WORKDIR /src/Ecommerce.WebApi
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Ecommerce.WebApi.dll"]
