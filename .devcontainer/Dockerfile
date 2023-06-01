# For details on usage and latest versions, see:
# https://mcr.microsoft.com/en-us/product/devcontainers/dotnet/about
FROM mcr.microsoft.com/vscode/devcontainers/dotnet:8.0-bookworm

# For dotnet-adr - currently non-functional due to https://github.com/endjin/dotnet-adr/issues/203
# # Install other dotnet versions (required for adr tool and dotnet suggest)
# RUN curl -sSL https://dot.net/v1/dotnet-install.sh > /tmp/dotnet-install.sh \
#     && chmod u+x /tmp/dotnet-install.sh

# RUN sudo /tmp/dotnet-install.sh --install-dir /usr/share/dotnet --channel 6.0 --runtime dotnet

# [Choice] Node.js version: none, lts/*, 16, 14, 12, 10
ARG NODE_VERSION="lts/*"
RUN if [ "${NODE_VERSION}" != "none" ]; then su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

# [Optional] Uncomment this section to install additional OS packages.
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
        python3 python-is-python3 python3-pip python3-dev \
# These are for running vscode insiders in the background to execeute extension tests
        xvfb libgtk-3-0 libnss3-dev libgbm-dev libasound2 libcanberra-gtk-module libcanberra-gtk3-module
     
# [Optional] Uncomment this line to install global node packages.
RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g aws-cdk-local aws-cdk" 2>&1

RUN sudo curl https://awscli.amazonaws.com/awscli-exe-linux-aarch64.zip -o /tmp/awscliv2.zip && \
    unzip /tmp/awscliv2.zip -d /tmp && \
    /tmp/aws/install

RUN su - vscode -c "pip install localstack awscli-local"

COPY setup_tools.sh /tmp/setup_tools.sh
RUN chmod +x /tmp/setup_tools.sh && su - vscode -c "/tmp/setup_tools.sh >> /tmp/setup_tools.log"

# This must be last in the .bashrc to ensure the paths are adjusted earlier
# the `tr -d '\015' is to workaround https://github.com/dotnet/command-line-api/pull/2186
# it can be removed when they release a version higher than 1.1.415701
# Temporarily removing this script, as it doesn't seem to work with any
# of the currently used dotnet tools.  Try again when there is a version
# higher than 1.1.415701
# RUN su - vscode -c "dotnet-suggest script bash | tr -d '\015' >> ~/.bashrc"

COPY post_start_command.sh /home/vscode/post_start_command.sh
RUN chmod +x /home/vscode/post_start_command.sh

ENV CONTEXTIVE_DEBUG=true
ENV DISPLAY=:99
