# Release Strategy 

* Status: accepted
* Deciders: Chris Simon
* Date: 2022-01-07

## Decision Summary

Use the [semantic-release](https://www.npmjs.com/package/semantic-release) package to manage version changes, change logs and release activities.  Use branches for feature development and merge to `main` when a feature is complete.

When ready to cut a release, merge to `release`, which will trigger a new release using [semantic-release](https://www.npmjs.com/package/semantic-release).

Since `semantic-release` doesn't support major versions < 1, a v1.0.0 tag will be added manually to the latest manually created release to serve as the baseline for the first `semantic-release` managed release.

## Context and Problem Statement

There are two areas of Release management that are determined in this ADR:

### Release Trigger

A process needs to be created to determine when a version is ready for release and initiating the release activities for that version.

### Release Process

When a release is triggered, a series of steps needs to be actioned:

1. Validating the proposed release (automated tests)
2. Compiling and packaging the Language Server for all target platforms & architectures
3. Preparing release notes
4. Committing & pushing changes to the version number (package.json) and changelog into the repo, tagging the commit with the release
5. Packaging the Visual Studio Code extension (including the new version number in the package)
6. Creating a github release, including the packages as files attached to the release
7. Uploading the extension packages (1 per target platform) to the VS Code Extension marketplace(s)

### General Context

* Processes that require manual steps are likely to be inconsistently executed
* There is a preference to invest time in developing software unique to Contextive and leveraging existing systems and tools for common/generic functionality
* There is currently no need for a pre-release or beta channel (this may change in the future)

## Decision Drivers 

Key decision drivers are:

* Amount of manual effort (lower the better)
* Investment in custom build of tooling or automation (lower the better)
* Alignment with industry standards/practices

## Considered Options

### Release Trigger

1. manual
2. Commits to `main` branch
3. Commits to a `release` branch
4. Tag convention

### Release Process

1. Manual
2. Custom automation
3. [semantic-release](https://www.npmjs.com/package/semantic-release)

## Decision Outcome

1. Release Trigger from commits to `release` branch
   1. Initial experiments with releasing off every commit to `main` indicated that there would be too high a release frequency, prompting too many updates and a more complex change log. By controlling releases with a manual merge from `main` to `release` we can review over time what a reasonable frequency/size of releases are.
   2. Commits to `main` are to be done by [squash merges](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/incorporating-changes-from-a-pull-request/about-pull-request-merges#squash-and-merge-your-pull-request-commits) from a merge request from a feature branch, with the squash merge commit message using [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/)
2. Release Process managed by [semantic-release](https://www.npmjs.com/package/semantic-release)
   1. because it provides repeatable, industry conventional approaches and already includes plugins for all the services currently needed.

### Positive Consequences 

* Maintainers will have control over release schedule - in the future, this may be further automated
* All mechanisms involved in releasing will be fully automated, making releasing a very simple task (simply merge from `main` to `release`).
* Semantic-release necessitates the use of [conventional commits](https://www.conventionalcommits.org/en/v1.0.0/) which will improve the quality and consistency of git commit logs
* Change logs/release notes will be based on git commit history automatically

### Negative Consequences

* Although there are tools to enforce the conventional commit approach when committing locally, squash merges of merge requests via the github UI do not allow to enforce a commit message structure.  
* [semantic-release](https://www.npmjs.com/package/semantic-release) is tailored for javascript packages.  Contextive expects to have other types of packages in the future (e.g. Visual Studio extension) which may not be as amenable to it's use.  This will need to be revisited when that extension is added.

## Pros and Cons of the Options

### Release Trigger

#### Manual

A human clicks a button to initiate a release from the current head of a nominated branch.

* Good, because full control over the release schedule
* Bad, because releases may be inconsistent
* Bad, because extra effort spent determining the need for a release and initiating a release

#### Commits to `main` branch

Any commit to `main` will initiate a release process.

* Good, because releases are predictable & consistent
* Good, because the process is simple and easy to understand
* Bad, because there may be multiple commits to `main` in a day, which may create too many releases, negatively affecting users with automatic updates turned on

#### Commits to a `release` branch

Any commits to a dedicated `release` branch will initiate a release process

* Good, many commits can accumulate on `main` before being released, so the release cadence can be more measured than the commit cadence
* Bad, because a human still needs to decide when to merge to `release`, or extra automation is required to trigger a merge to `release` at a given time

### Release Process

#### Manual

* Bad, because lots of effort, which will likely lead to infrequent releases
* Bad, because likely to lead to inconsistent or incorrect steps

#### Custom Automation

* Good, because less effort to create a release
* Bad, because extra effort to create the custom automation
* Bad, because the custom automation will need maintaining

#### [semantic-release](https://www.npmjs.com/package/semantic-release)

* Good, because less effort to create a release
* Good, because plugins already exist to carry out all the required steps, so less effort to configure
* Good, because it enforces conventional commits, improving commit history quality
* Bad, because uncertain how it will work with non-JavaScript package types (e.g. Visual Studio extension)