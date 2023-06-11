#!/usr/bin/bash
# Start Xvgb so extension tests can run in the background from the test explorer
echo "Display: \""$DISPLAY"\""
rm /tmp/$DISPLAY-lock -f
/usr/bin/Xvfb $DISPLAY -screen 0 1024x768x24 > /dev/null 2>&1 &
sudo service dbus start
dbus-daemon --session --address=$DBUS_SESSION_BUS_ADDRESS --nofork --nopidfile --syslog-only &

pushd src
dotnet tool restore
popd

find ./ -type f -name "*.fsproj" -print0 | xargs -r0 /bin/bash -c 'for f in "$@"; do echo "$f"; dotnet restore "$f"; done;' ''

cd src/vscode/contextive
npm install

aws configure set aws_access_key_id 0000 --profile local
aws configure set aws_secret_access_key 0000 --profile local
aws configure set region us-east-1 --profile local