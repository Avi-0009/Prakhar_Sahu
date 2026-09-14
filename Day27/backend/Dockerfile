# =============================================================================================
# The modular monolith, built as one image.
#
# 22 projects, one deployable. The whole solution is copied and restored, then only the host
# project is published -- project references pull in exactly the modules it needs and nothing
# else. `dotnet publish` on the host produces one output directory containing every module
# assembly, which is what "modular monolith" means at deployment time: the boundaries are
# compile-time facts, and the artefact is a single process.
#
# The build context is the SOLUTION directory (backend/), not the host project. Copying only
# the host project would fail restore on eighteen missing references.
# =============================================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project files first, so a change to a .cs file does not invalidate the restore layer.
COPY QuotesApi.slnx ./
COPY src/Directory.Build.props ./src/
COPY src/**/*.csproj ./
COPY . ./

RUN dotnet restore src/QuotesApi.Host/QuotesApi.Host.csproj
RUN dotnet publish src/QuotesApi.Host/QuotesApi.Host.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# The assembly is still QuotesApi.dll -- see the AssemblyName in QuotesApi.Host.csproj. The
# verification scripts and the container both find the process by that name.
ENTRYPOINT ["dotnet", "QuotesApi.dll"]
