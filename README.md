# mixtapeFS
filesystem with media transcoding on-demand

## what
* select some folders with music and they'll appear in drive M:\
* ...except all `.opus` files have become `.opus.flac` files
* try to open one and FFmpeg pops up converting it

## status
* POC (everything's hardcoded but it works?? kinda)

## usecases
* play opus files in traktor
* play ogg and opus files in rekordbox
* play broken mp3 files in traktor (it fixes metadata too)

## dependencies
* https://github.com/dokan-dev/dokany/releases/latest
* https://ffmpeg.zeranoe.com/builds/

