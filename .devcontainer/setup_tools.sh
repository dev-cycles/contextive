# Ensure npm is available
source /usr/local/share/nvm/nvm.sh
# Install npm globals
npm install -g yo generator-code vsce
# Install dotnet global tools
PATH=/home/vscode/.dotnet/tools:$PATH
dotnet new --install Fable.Template
dotnet tool install dotnet-suggest --global
dotnet tool install paket --global
dotnet tool install fable --global
dotnet tool install femto --global
# no stable version of the adr tool available yet available yet \n\
dotnet tool install adr --global --version 0.1.0-preview.36
adr templates default set madr

echo "export PATH=$PATH" >> /home/vscode/.bashrc