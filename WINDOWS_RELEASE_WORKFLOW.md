# Pending Windows release automation

The repository stays active so the
[Pending Windows release workflow](.github/workflows/pending-windows-release.yml)
can check [Gyan's source request](https://github.com/GyanD/codexffmpeg/issues/254)
daily at **09:00 UTC**. GitHub can delay scheduled runs and disable public-repo
schedules after 60 days without repository activity; this is not a guaranteed
release date.

The scheduled job reports upstream changes on
[the publication gate](https://github.com/TheImortalimp/AI-WARP-ENGINE/issues/1).
It does not infer licensing compliance from a reply, closed issue or source URL.
No scheduled job publishes the installer.

## Private build delivery

The current installer was uploaded to a private review repository. GyanD was
invited; delivery access depends on accepting that invitation. Personal GitHub
repositories require collaborator write access, authorized only on the isolated
review repository. The public project's release environment is not shared.

A separate unpublished maintainer-only `windows-6.4.37-staging` draft release
holds the same installer for automated publication. **Never publish that draft.**
It is not a public download.

Installer SHA256:
`1ae628213305aabc3c0c348dc0ae19fd3f46de877474441233f47ed006a1dae3`.
Bundled FFmpeg SHA256:
`ad8f211bc894755e0061c55ab280ae00e8d3d4f15a8cc4372b24cfa247b5942e`.

## Approval and automatic publication

1. Verify complete corresponding sources for the exact FFmpeg binary: all
   covered static/transitive dependencies, patches, notices and build inputs.
   Prepare a complete public HTTPS ZIP and calculate its SHA256.
2. Run **Pending Windows release** from GitHub Actions on `main`. Set `publish`
   to true and supply `source_url` and `source_sha256`. Do not put credentials
   or signed/private download tokens in these inputs.
3. Review and approve the `windows-source-reviewed` environment as
   TheImortalimp. Admin bypass is disabled; only `main` is allowed.
   Self-review is allowed so the owner can queue and then approve the run.
4. The approved job checks environment protection, staged installer hash and
   source ZIP hash/integrity. Archive validity is not a substitute for your
   source-coverage review.
5. It publishes the source ZIP before the installer, refreshes checksums,
   attaches `WINDOWS_RELEASE_APPROVAL.md` and updates the v6.4.37 release.
   The approval record supersedes the historical pending notices in older docs.

The workflow does not run the installer or source build scripts. It publishes
the complete installer, not a standalone EXE-only portable app. Keep the
existing UAC, unsigned-installer, WebView2, policy and uninstall warnings.

If a step fails, inspect the Actions error and release assets before retrying.
Uploads are replaceable; a failed job can leave already uploaded assets, including
a source ZIP or installer if it fails after those publication steps.
The workflow never moves the v6.4.37 tag or edits website-root home/index pages.
