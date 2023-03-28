#
# Install the .NET7 Software Development Kit (SDK):
#
FROM mcr.microsoft.com/dotnet/sdk:7.0 as builder

#
# Install pgroutiner global tool with latest version:
# Note, to use a different version add --version <number> command line parameter to this line
#
RUN dotnet tool install --global dotnet-pgroutiner --version 5.3.3

#
# Now, Install .NET7 runtime, which is a significantly smaller image:
#
FROM mcr.microsoft.com/dotnet/runtime:7.0

#
# Set working dir to home dir. This dir will be used for mounting configurations:
#
WORKDIR /home

#
# Copy all global tools to /opt/bin, since tools are complied on install and path to /opt/bin:
#
COPY --from=builder /root/.dotnet/tools/ /opt/bin
ENV PATH="/opt/bin:${PATH}"

#
# Install wget and gnupg needed for the repository configuration
#
RUN apt-get update && apt-get install -y wget gnupg

#
# Create the file repository configuration for postgresql client tools:
#
RUN sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt bullseye-pgdg main" > /etc/apt/sources.list.d/pgdg.list'

#
# Import the repository signing key for postgresql client tools:
#
RUN wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add -

#
# Install postgresql client tools for version 15:
# Note: change postgresql-client-15 for desired version.
#
RUN apt-get update && apt-get install -y postgresql-client-15 && apt-get install -y postgresql-client-14 && apt-get install -y postgresql-client-13 && apt-get install -y postgresql-client-12 && apt-get install -y postgresql-client-11 && apt-get install -y postgresql-client-10 && apt-get install -y postgresql-client-9.6

#
# Run pgroutiner on entry
#
# Examples:
#
# - check the version
# docker run --rm -it pgroutiner --version
#
# - mount current directory (with configuration) and run (linux)
# docker run --rm -it -v $(pwd):/home/ pgroutiner 
#
# - mount current directory (with configuration) and run (powershell)
# docker run --rm -it -v ${PWD}:/home/ pgroutiner 
#
ENTRYPOINT ["pgroutiner"]
