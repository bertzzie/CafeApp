#!/bin/bash
if test "$OS" = "Windows_NT"
then # For Windows
    fake -v run build.fsx
else # For Non Windows
    fake -v run build.fsx
fi
