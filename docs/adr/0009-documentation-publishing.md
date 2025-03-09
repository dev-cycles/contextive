# Documentation Publishing

* Status: accepted
* Deciders: Chris Simon
* Date: 2024-11-29

## Decision Summary

To deploy new version documentation to a subfolder of the main site by hosting in Cloudfront + S3 and on release of a new extension version, deploying the current state of the documentation site to subfolder/subsite of the main documentation site (e.g. https://docs.contextive.tech/v1.10.1).  Each version's documentation site will be retained in full side-by-side with all assets as a complete archive.

A changelog page will contain links to each version and a home landing page will direct users to the latest version (by redirect or explicit link).

## Context and Problem Statement

In ADR-0008 it was decided to use [Starlight](https://starlight.astro.build/) as a documentation framework.  Startlight will help produce a static site, comprising a folder of html, css & images and other assets.

Having produced such a folder, it's then necessary to publish it for public consumption.

In addition, as a deployed plugin where users can control their own upgrade schedule, it's important to maintain publicly accessible version-specific documentation.  When documentation was simple markdown files in github, this was simple by just linking to git commit that contained the documentation.  With a separately hosted website, an alternative approach is needed.

As the specific approach taken for making historical documentation version available depends on capbilities offered by the hosting provider, these two decisions are combined in this ADR.

## Decision Drivers

* Free or low cost hosting
* Ease of deployment/publishing step
* Ability to host version-specific documentation and guide users to the documentation for their version
* Ease of co-hosting with other forthcoming Contextive documentation, such as for Contextive Cloud/Saas products and other components (slackbot, browser extension)

## Considered Options

### Hosting Provider

* [Netlify](https://www.netlify.com/)
* [Cloudflare Pages](https://pages.cloudflare.com/)
* [Github Pages](https://pages.github.com/)
* [Vercel](https://vercel.com/)
* [AWS Cloudfront](https://aws.amazon.com/cloudfront/) + [AWS S3](https://aws.amazon.com/s3/)
* [Azure CDN](https://azure.microsoft.com/en-us/products/cdn) + [Azure Static Website](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-static-website)

### Historical Version Documentation Hosting

* Deploy to separate sites hosted on a subdomain, e.g. https://1-10-1.docs.contextive.tech for v1.10.1
* Deploy to subfolder of main site, e.g. https://docs.contextive.tech/v1.10.1
  * Hosting provider must support deploying to subfolders (e.g. AWS Cloudfront + S3, or Azure CDN + Azure Static Websites)
  * If the hosting provider doesn't support deploying to a subfolder, workarounds include:
    * Deploy to a separate site and proxy from /{version} to https://{version}.docs.contextive.tech
    * Commit generated output to a subfolder (same or separate repo) and deploy the whole folder (containing current and all past version generated docs that are all committed in repo) to a single site

## Decision Outcome

* Selected Hosting Provider:
  * [AWS Cloudfront](https://aws.amazon.com/cloudfront/) + [AWS S3](https://aws.amazon.com/s3/)  
* Selected Historical Version Documentation Approach:
  * Deploy directly to a subfolder of the main site, e.g. https://docs.contextive.tech/v1.10.1

Secondary decisions required due to the primary decisions:

* Infrastructure Management Approach
  * Infrastructure configuration & deployment to be maintained in a private repository
  * Infrastructure to define an OIDC permission for Guthub & least privilege Role for Github
* Deployment Approach
  * Public Contextive open source repository to contain the extension documentation
  * Public Contextive open source repository's Github Action to obtain an AWS OIDC token allowing to identify the target S3 bucket and deploy to a subfolder
  * Commits to main will deploy to a 'test' site (https://docs.test.contextive.tech) with version "v{current}+{short-sha}", e.g. https://docs.test.contextive.tech/v1.10.1+ddb2dbe2
  * Releases will deploy to live site with the new version code, e.g. https://docs.test.contextive.tech/v1.10.2

#### Positive Consequences

* Highly flexible deployment model
* No risk of exhausting site limits with a site-per-version approach
* Lower latency and configuration complexity than having to proxy to a dedicated site
* No need to maintain historical site content committed to the repository or to redeploy an ever growing set of static assets each time

#### Negative Consequences

* While usage should fit within the AWS free tier, it may exceed it at some point and start incurring some costs.
* Higher setup effort to provision, configure & secure the required AWS resources.

## Pros and Cons of the Options

As many options are quite similar, the pros and cons are analyzed in two groups:

### Dedicated Static Site hosting services: Netlify, Vercel, Github Pages, Cloudflare Pages

Pros:
* Ease of hosting and deployment (no infra configuration or security)
* Simple deployment

Cons:
* Costs can rise sharply if traffic rises
* Inflexible with historical version hosting, requiring complex separate site or proxy setups, OR requiring to redeploy _every_ version on every update.


### Generic Cloud Platforms: AWS, Azure

Pros:
* Flexibility, particularly regarding co-hosting with other components and historical version hosting
* Cons are likely to rise linearly with usage, but hopefully will stay within the free tier for a while

Cons:
* Complex to setup & secure infrastructure configuration

Chose:
* AWS - primarily because of existing familiarity with the platform and ability to use the CDK infrastructure configuration tooling with F# (the primarily language of choice for this product).