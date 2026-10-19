"""Small first-party CC0 previews, no account, cookies, keys, or paid service.

Freesound exposes each preview URL in its public share widget. These are the
published compressed previews, NOT the original masters behind account login.
This distinction remains explicit in the manifest and all derived material.
"""
from pathlib import Path
import hashlib, html, json, re, urllib.request

ROOT = Path(__file__).resolve().parent
SOURCES = [
    ("556499", "cookies%2Bpolicy", "Rotary Phone Ring loopable", "An actual rotary telephone ring; uploader also supplies matching handset contacts."),
    ("556496", "cookies%2Bpolicy", "Rotary Phone Handpiece Soft", "Picking up and then hanging up softly on the same old rotary telephone."),
]
LICENSE = "https://creativecommons.org/publicdomain/zero/1.0/"

def get(url, limit):
    with urllib.request.urlopen(urllib.request.Request(url, headers={"User-Agent": "EchoProtocol audio provenance review"}), timeout=40) as response:
        data = response.read(limit + 1)
        if len(data) > limit:
            raise RuntimeError("Download exceeds small-asset limit: " + url)
        return data

def sha(data): return hashlib.sha256(data).hexdigest()

if __name__ == "__main__":
    rows = []
    for identity, author, title, description in SOURCES:
        url = f"https://freesound.org/people/{author}/sounds/{identity}/"
        page = get(url, 1_000_000).decode("utf-8")
        if not re.search(r'href=[\"\']https?://creativecommons.org/publicdomain/zero/1.0/', page):
            raise RuntimeError("No explicit CC0 deed on current source page: " + url)
        match = re.search(r'data-static-file-url="([^"]+)"', page)
        if not match: raise RuntimeError("No public preview URL: " + url)
        preview = html.unescape(match.group(1))
        if not preview.startswith("https://cdn.freesound.org/previews/"):
            raise RuntimeError("Unexpected preview origin")
        data = get(preview, 2_000_000)
        filename = preview.rsplit("/", 1)[-1]
        path = ROOT / "Source" / filename
        path.write_bytes(data)
        row = dict(id=identity, author=author, title=title, source_page=url,
                   description_paraphrase=description, license="CC0 1.0", license_url=LICENSE,
                   reviewed_date="2026-10-01", download_url=preview,
                   local_file=str(path.relative_to(ROOT)), source_kind="public HQ MP3 preview; not original lossless master",
                   bytes=len(data), sha256=sha(data),
                   attribution=f"{title} by {author} (Freesound), CC0 1.0.",
                   human_audition="NOT PERFORMED; agent cannot receive audio in this session",
                   selection_state="prepared candidate; not imported or human-approved")
        # Sanitize to relevant public license/identity data only. Do not persist
        # full HTML, site scripts, visitor identifiers, or authentication fields.
        evidence = json.dumps({key: row[key] for key in ["id", "author", "title", "source_page", "description_paraphrase", "license", "license_url", "reviewed_date", "download_url", "source_kind"]}, indent=2)
        evidence_path = ROOT / "Evidence" / (identity + ".json")
        evidence_path.write_text(evidence + "\n")
        row["evidence_file"] = str(evidence_path.relative_to(ROOT))
        row["evidence_sha256"] = sha(evidence_path.read_bytes())
        rows.append(row)
        print(identity, len(data), sha(data))
    (ROOT / "sources.json").write_text(json.dumps(rows, indent=2) + "\n")
