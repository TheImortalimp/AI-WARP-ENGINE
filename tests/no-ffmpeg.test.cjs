'use strict';

// The native editions convert media with Windows components only. This keeps every external-converter reference
// out of shipped code, build scripts and tests; only history, legal notices and the older gated workflow may name it.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { test } = require('node:test');

const root = path.resolve(__dirname, '..');
const EXTERNAL = /ffmpeg|ffprobe|ffplay|gyan/i;
const TEXT_FILE = /\.(?:md|html|json|cjs|js|cs|csproj|ps1|nsi|txt|yml|yaml|css|svg|py|rtf)$/i;
const SKIPPED_DIRECTORIES = new Set(['.git', 'node_modules', 'bin', 'obj']);

// Release history, licensing records for the withheld 6.4.37 binaries and the older approval-gated workflow.
const ALLOWED_FILES = new Set([
    'CHANGELOG.md',
    'RELEASE_NOTES_6.4.37.md',
    'RELEASE_NOTES_6.4.37.1.md',
    'RELEASE_NOTES_6.4.38.1.md',
    'RELEASE_NOTES_6.4.38.2.md',
    'RELEASE_NOTES_6.4.38.21.md',
    'THIRD_PARTY_NOTICES.md',
    'WINDOWS_RELEASE_WORKFLOW.md',
    '.github/workflows/pending-windows-release.yml',
    'tests/no-ffmpeg.test.cjs'
]);

// The build rejects a package that contains any external converter binary.
const BUILD_GUARD_LINES = [
    /Where-Object \{ \$_\.Name -match 'ffmpeg\|ffprobe\|ffplay' \}/,
    /throw 'Native package unexpectedly contains FFmpeg files\.'/
];

function* walk(directory) {
    for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
        const full = path.join(directory, entry.name);
        const relative = path.relative(root, full).split(path.sep).join('/');
        if (entry.isDirectory()) {
            if (SKIPPED_DIRECTORIES.has(entry.name) || /^shell\/publish/.test(relative)) continue;
            yield* walk(full);
        } else if (TEXT_FILE.test(entry.name)) {
            yield relative;
        }
    }
}

test('shipped code, build scripts, tests and current docs never reference an external converter', () => {
    const offenders = [];
    let scanned = 0;
    for (const relative of walk(root)) {
        if (ALLOWED_FILES.has(relative)) continue;
        scanned++;
        const lines = fs.readFileSync(path.join(root, relative), 'utf8').split(/\r?\n/);
        lines.forEach((line, index) => {
            if (!EXTERNAL.test(line)) return;
            if (relative === 'shell/build.ps1' && BUILD_GUARD_LINES.some(guard => guard.test(line))) return;
            offenders.push(`${relative}:${index + 1}: ${line.trim().slice(0, 100)}`);
        });
    }
    assert.ok(scanned > 40, `Only ${scanned} files were scanned; the walker is broken.`);
    assert.deepEqual(offenders, []);
});

test('the native build still rejects any bundled external converter binary', () => {
    const script = fs.readFileSync(path.join(root, 'shell', 'build.ps1'), 'utf8');
    for (const guard of BUILD_GUARD_LINES) assert.match(script, guard);
});

test('the native host never launches an external process', () => {
    for (const file of fs.readdirSync(path.join(root, 'shell')).filter(name => name.endsWith('.cs'))) {
        const source = fs.readFileSync(path.join(root, 'shell', file), 'utf8');
        assert.doesNotMatch(source, /Process\.Start|ProcessStartInfo|new Process\(|UseShellExecute/, file);
    }
});

test('native conversion is the single Windows MediaTranscoder backend', () => {
    const converter = fs.readFileSync(path.join(root, 'shell', 'LocalMediaConverter.cs'), 'utf8');
    assert.match(converter, /MediaTranscoder/);
    const project = fs.readFileSync(path.join(root, 'shell', 'AiWarpEngine.csproj'), 'utf8');
    assert.doesNotMatch(project, /<Content Include="[^"]*\.exe"/i);
});

test('generated native publish folders contain no external converter files', () => {
    const shell = path.join(root, 'shell');
    const folders = fs.readdirSync(shell, { withFileTypes: true })
        .filter(entry => entry.isDirectory() && /^publish-native-/.test(entry.name));
    for (const folder of folders) {
        const pending = [path.join(shell, folder.name)];
        while (pending.length) {
            for (const entry of fs.readdirSync(pending.pop(), { withFileTypes: true })) {
                const full = path.join(entry.parentPath || entry.path, entry.name);
                if (entry.isDirectory()) pending.push(full);
                else assert.doesNotMatch(entry.name, /ffmpeg|ffprobe|ffplay/i, full);
            }
        }
    }
});
