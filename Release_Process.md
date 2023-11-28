# Release Process

This outlines the general release process that I (CompSciLauren) do with each new production release. If you are contributing to this project, you don't need to worry about this, but feel free to read on if you're curious. This file exists simply for documentation purposes.

## Project Management

Story board for tracking what's being worked on or planned to be worked on can be found here: https://github.com/users/CompSciLauren/projects/4/views/1

This mainly just tracks whatever I happen to be doing, and not really used for tracking what anyone else is potentially contributing.

## Branching Strategy

### Branches

* master - production-ready branch
* development - testing branch
* DS-123 - individual story branch naming convention (where `123` is the GitHub Issue being worked on)

### General Process

1. Checkout new branch, `master` --> `DS-123` // create a new branch
2. Add the code changes. Commit messages are structured like "DS-123 Add the thing" (message should be accurate high level description of the changes in the commit)
3. Merge branch, `DS-123` --> `development` // merge into dev branch for testing
4. Create PR, `DS-123` --> `master` // after completed test, can merge into production-ready branch

## Doing the Release

Here are the steps for the release. Note the file used for the release is found in the project bin folder, looks like `DailyScreenshot 3.0.0.zip`.

- [x] All code changes intended to be released are merged to `master` branch
    - [x] Includes correct version in [manifest.json](./DailyScreenshot/manifest.json) file. (Follows [semver](https://semver.org/) versioning standard)
- [x] Draft a new Nexus Article with release notes
- [x] [Publish a New GitHub Release](https://github.com/CompSciLauren/stardew-valley-daily-screenshot-mod/releases/new)
- [x] Publish new version on Nexus
    - [x] Include updating changelog
- [x] Publish Nexus Article with release notes
- [x] Publish new version on the other mod sites (including changelog)
    - [x] [Chucklefish](https://community.playstarbound.com/resources/daily-screenshot.5907/)
    - [x] [CurseForge](https://www.curseforge.com/stardewvalley/mods/daily-screenshot)
    - [x] [ModDrop](https://www.moddrop.com/stardew-valley/mods/677025-daily-screenshot)
- [x] Close out GitHub Issues that are now released
- [x] Respond to any related Nexus bug reports and comments
- [x] Add a comment about the release to SDV Discord in #modded-farmers channel
    - [x] Remember to include a picture (can use main cover photo from Nexus site)
- [x] Right click the comment --> Apps --> Publish, to have it be posted in #mod-showcase channel