#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS base
RUN apk add --no-cache git openssh-client curl &&\
    git config --global user.name approved_user &&\
    git config --global user.email approved@corp.com &&\
    curl --location --output /usr/local/bin/release-cli "https://gitlab.com/api/v4/projects/gitlab-org%2Frelease-cli/packages/generic/release-cli/latest/release-cli-linux-amd64" &&\
    chmod +x /usr/local/bin/release-cli

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
ARG VERSION=0.0.0.0
WORKDIR /src
COPY . ./
RUN dotnet publish "./src/SemanticReleaseCLI/SemanticReleaseCLI.csproj" -c ${BUILD_CONFIGURATION} -p:Version=${VERSION} -r linux-musl-x64 -o /app/publish

FROM base AS final
COPY --from=build /app/publish "/usr/local/bin"
