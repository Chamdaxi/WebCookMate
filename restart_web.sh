#!/bin/bash

# Script to restart web application

echo "🛑 Stopping existing web processes..."
pkill -f "dotnet run" || echo "No existing processes found"

echo "⏳ Waiting 2 seconds..."
sleep 2

echo "🔨 Building project..."
cd /Users/phamtau/WebCookmate
export PATH="$HOME/.dotnet:$PATH"
dotnet build

if [ $? -eq 0 ]; then
    echo "✅ Build succeeded!"
    echo "🚀 Starting web application..."
    dotnet run
else
    echo "❌ Build failed! Please fix errors first."
    exit 1
fi

