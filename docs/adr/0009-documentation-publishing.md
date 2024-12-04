# Documentation Publishing

* Status: accepted
* Deciders: Chris Simon
* Date: 2024-11-29

## Decision Summary

## Context and Problem Statement

In ADR-0008 it was decided to use [Starlight](https://starlight.astro.build/) as a documentation framework.  Startlight will help produce a static site, comprising a folder of html, css & images and other assets.

Having produced such a folder, it's then necessary to publish it for public consumption.

In addition, as a deployed plugin where users can control their own upgrade schedule, it's important to maintain publicly accessible version-specific documentation.  When documentation was simple markdown files in github, this was simple by just linking to git commit that contained the documentation.  With a separately hosted website, an alternative approach is needed.

As the specific approach taken for making historical documentation version available depends on capbilities offered by the hosting provider, these two decisions are combined in this ADR.

## Decision Drivers

* Free or low cost hosting
* Ease of deployment/publishing step
* Ability to host version-specific documentation and guide users to the documentation for their version

## Considered Options

### Hosting Provider

* Netlify
* Cloudflare Pages
* Github Pages
* Vercel

### Historical Version Documentation Hosting

* Deploy to separate sites hosted on a subdomain, e.g. https://1-10-1.docs.contextive.tech for v1.10.1
* Deploy to subfolder of main site, e.g. https://docs.contextive.tech/v1.10.1
  * Many static site hosting providers don't support deploying to a subfolder, so there are a few options for how to achieve this:
    * Deploy to a separate site and proxy from /{version} to https://{version}.docs.contextive.tech
    * Commit generated output to a subfolder (same or separate repo) and deploy the whole folder (containing current and all past version generated docs) to a single site

## Decision Outcome

### Hosting Provider

Netlify

#### Positive Consequences

#### Negative Consequences

### Historical Version Documentation Hosting

Commit generated output to a subfolder (same or separate repo) and deploy the whole folder (containing current and all past version generated docs) to a single site

#### Positive Consequences

#### Negative Consequences

## Pros and Cons of the Options

### Netlify

### Option 2
