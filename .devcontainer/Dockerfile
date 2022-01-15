FROM mcr.microsoft.com/vscode/devcontainers/dotnet:0-6.0-focal

# Install other dotnet versions (required for adr tool and dotnet suggest)
RUN curl -sSL https://dot.net/v1/dotnet-install.sh > /tmp/dotnet-install.sh \
    && chmod u+x /tmp/dotnet-install.sh
RUN sudo /tmp/dotnet-install.sh --install-dir /usr/share/dotnet --channel 5.0 --runtime dotnet
RUN sudo /tmp/dotnet-install.sh --install-dir /usr/share/dotnet --channel 3.1 --runtime dotnet

# [Choice] Node.js version: none, lts/*, 16, 14, 12, 10
ARG NODE_VERSION="lts/*"
RUN if [ "${NODE_VERSION}" != "none" ]; then su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

# [Optional] Uncomment this section to install additional OS packages.
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends git

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1

COPY setup_tools.sh /tmp/setup_tools.sh
RUN chmod +x /tmp/setup_tools.sh && su - vscode -c "/tmp/setup_tools.sh"

# This must be last in the .bashrc to ensure the paths are adjusted earlier
COPY _dotnet_bash_complete.sh /tmp/_dotnet_bash_complete.sh
RUN su - vscode -c "cat /tmp/_dotnet_bash_complete.sh >> /home/vscode/.bashrc"

COPY post_start_command.sh /home/vscode/post_start_command.sh
RUN chmod +x /home/vscode/post_start_command.sh