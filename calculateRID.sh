#!/bin/bash
arch="`uname -m`"
RID="linux-x64"


if [ "$arch" = "aarm64" ]; then
    echo "linux-arm64"
    exit
fi

if [ "$arch" = "arm64" ]; then
    echo "linux-arm64"
    exit
fi

if [ "$arch" = "arm32" ]; then
    echo "linux-arm32"
    exit
fi

if [ "$arch" = "x86_64" ]; then
    echo "linux-x64"
    exit
fi
