#
# Install the .NET7 Software Development Kit (SDK)
#
FROM mcr.microsoft.com/dotnet/sdk:7.0 as builder

#
# Install pgroutiner global tool with latest version.
# Note, to use a different version add --version <number> command line parameter to this line
#
RUN dotnet tool install --global dotnet-pgroutiner

#
# Now, Install .NET7 runtime, which is a significantly smaller image
#
FROM mcr.microsoft.com/dotnet/runtime:7.0

#
# Copy all global tools to /opt/bin, since tools are complied on install and path to /opt/bin
#
COPY --from=builder /root/.dotnet/tools/ /opt/bin
ENV PATH="/opt/bin:${PATH}"

#
# Set working dir to home dir. This dir will be used for mounting configurations.
#
WORKDIR /home

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