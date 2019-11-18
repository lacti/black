#!/bin/bash

#security unlock-keychain -p ${!KEYCHAIN_PASSWORD}

cd build

xcodebuild \
    -scheme Unity-iPhone \
    archive \
    -archivePath build \
    CODE_SIGN_STYLE="Manual" \
    PROVISIONING_PROFILE_SPECIFIER="Black Ad Hoc" \
    CODE_SIGN_IDENTITY="Apple Distribution: GEOYEOB KIM (TG9MHV97AH)"

xcodebuild \
    -exportArchive \
    -exportOptionsPlist ../exportoptions.plist \
    -archivePath "build.xcarchive" \
    -exportPath "build"

