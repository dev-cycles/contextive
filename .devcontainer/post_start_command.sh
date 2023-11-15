# Start Xvgb so extension tests can run in the background from the test explorer
rm /tmp/.X99-lock -f
/usr/bin/Xvfb :99 -screen 0 1024x768x24 > /dev/null 2>&1 &

find ./ -type f -name "*.fsproj" -print0 | xargs -r0 /bin/bash -c 'for f in "$@"; do echo "$f"; dotnet restore "$f"; done;' ''
dotnet tool restore
cd src/vscode/contextive
npm install
