# mixtapeFS
filesystem with media transcoding on-demand

## what
* select some folders with music and they'll appear in drive M:\
* ...except all `.opus` files have become `.opus.flac` files
* try to open one and FFmpeg pops up converting it

## status
* has a GUI, works for the most part
* transcodes `opus` and `ogg` files
* traktor-specific features (metadata patching) are disabled in the release build

## usecases
* play opus files in traktor
* play ogg and opus files in rekordbox
* play broken mp3 files in traktor (it fixes metadata too)

## how to use
* in the `Mount:` field, choose a drive letter to use for the virtual filesystem
* in the `Cache:` field, specify how the cache should work, permitting files to be up to 30min old and spend up to 64gb for instance (defaults), also the location to use for the cache
* in the `Source:` field, provide the path to a folder that contains music (or folders with music)
* in the `Name:` field, provide a name to use for that `Source:` folder
* press `[Add new]` to register this pair of `Source:` and `Name:` into the mappings list below
* keep adding more folders to rehost if you'd like

## note
* avoid a memleak by running the exe separately (not using visualstudio) (nice)

## todo
* figure out what's causing traktor to crash on certain flac files and patch those too
  * probably won't bother since traktor is just too janky

## dependencies
* https://github.com/dokan-dev/dokany/releases/latest
* https://ffmpeg.zeranoe.com/builds/
