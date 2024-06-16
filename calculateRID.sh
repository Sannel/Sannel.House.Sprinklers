#!/bin/bash

b = "`uname -m`" == "aarm64" || "`uname -m`" == "arm64";
RID = "linux-x64"
if [ $b ] ; then
	RID = "linux-arm64"
fi
b = "`uname -m`" == "arm32";
if [ $b ] ; then
	RID = "linux-arm32"
fi

export RID