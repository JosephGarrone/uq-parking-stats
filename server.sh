#!/bin/bash
if [ "$EUID" -ne 0 ]
  then echo "Please run as root"
  exit
fi

DIR="/var/log/uq-parking/"
FOREVER_LOG="stats.forever.log"
STDOUT="stats.log"
STDERR="stats.log"
PID="stats.pid"

mkdir -p /var/log/uq-parking

if [ $1 = "stop" ]; then
    forever stop app.js
else
    PORT=8001 forever start -l $DIR$FOREVER_LOG -o $DIR$STDOUT -e $DIR$STDERR --pidFile $DIR$PID --append ./bin/www 
fi


