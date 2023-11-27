# Ensure npm is available
source /usr/local/share/nvm/nvm.sh
# Install npm globals
npm install -g yo generator-code vsce

# Install dotnet global tools
echo "export PATH=$PATH:~/.dotnet/tools" >> /home/vscode/.bashrc
PATH=$PATH:~/.dotnet/tools
echo 
dotnet new --install Fable.Template
dotnet tool install dotnet-suggest --global
dotnet tool install paket --global
dotnet tool install fable --global
dotnet tool install femto --global
dotnet tool install fantomas --global

push src
dotnet tool restore
pop

## dotnet-adr
# Currently non-functional due to https://github.com/endjin/dotnet-adr/issues/203

# dotnet tool install adr --global

# # Setup default adr template
# # Make sure .config exists in the home folder.  If .config doesn't exist yet,
# # the adr config ends up in $HOME/endjin which doesn't work once we're in the
# # container, because by then .config _does_ exist
# mkdir -p $HOME/.config
# adr templates default set madr