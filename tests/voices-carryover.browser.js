'use strict';

async function runVoiceCarryoverChecks() {
    const check = (condition, message) => { if (!condition) throw new Error(message); };
    const canvas = document.createElement('canvas');
    canvas.style.cssText = 'position:fixed;left:-1000px;width:160px;height:90px';
    document.body.appendChild(canvas);
    const engine = new AiWarpEngine(canvas);
    cancelAnimationFrame(engine.animFrameId);
    const source = document.createElement('canvas');
    source.width = canvas.width;
    source.height = canvas.height;
    const context = source.getContext('2d');
    for (let x = 0; x < source.width; x += 8) {
        context.fillStyle = x % 16 ? '#204080' : '#90b060';
        context.fillRect(x, 0, 8, source.height);
    }
    const original = context.getImageData(0, 0, source.width, source.height).data;
    engine.setSourceMode('image', source);
    engine.setAiWarpActive(false);
    engine.setGhostMode('OFF', 0);
    engine.setVsyncMode('OFF', 0);
    let now = 100000;
    let voiceFrames = 0;
    const voices = [];
    try {
        check(engine.coreEffectNames.length === 17, 'Core voice bank count changed.');
        check(engine.gc3EffectNames.length === 17, 'GC3 voice bank count changed.');
        check(new Set(engine.effectNames).size === 34, 'Missing/duplicate voice.');
        for (const name of engine.effectNames) {
            for (const other of engine.effectNames) engine.setEffectEnabled(other, false);
            engine.setEffectEnabled(name, true);
            engine.setEffectLevel(name, 0.8);
            engine.setEffectSize(name, 1.2);
            engine.setEffectDepth(name, 0.7);
            let visible = false;
            for (let frame = 0; frame < 24; frame++) {
                engine.mainRenderLoop(now += 100);
                cancelAnimationFrame(engine.animFrameId);
                check(engine.renderErrors.size === 0, `${name}: ${JSON.stringify([...engine.renderErrors])}`);
                const pixels = engine.gctx.getImageData(0, 0, canvas.width, canvas.height).data;
                const filters = [canvas.style.filter, document.documentElement.style.filter];
                visible ||= pixels.some((value, index) => value !== original[index]) ||
                    filters.some(filter => filter && filter !== 'none');
                voiceFrames++;
            }
            check(visible, `${name} produced no observable pixels/filter.`);
            voices.push(name);
        }
        for (const name of engine.effectNames) engine.setEffectEnabled(name, false);
        const systems = [];
        for (const mode of ['DIA', 'SPECTRUM', 'CRT', 'PLASMA', 'LED']) {
            check(engine.ghostModes.includes(mode), `Missing Ghost ${mode}.`);
            check(engine.setGhostMode(mode, 0.6) === mode, `Ghost ${mode} was not selected.`);
            engine.mainRenderLoop(now += 100);
            cancelAnimationFrame(engine.animFrameId);
            check(engine.renderErrors.size === 0, `Ghost ${mode} failed.`);
            systems.push('Ghost ' + mode);
        }
        engine.setGhostMode('OFF', 0);
        for (const mode of ['ON', 'GHOST_OFF', 'FULL']) {
            check(engine.vsyncModes.includes(mode), `Missing V-Sync ${mode}.`);
            check(engine.setVsyncMode(mode, 0.6) === mode, `V-Sync ${mode} was not selected.`);
            engine.mainRenderLoop(now += 100);
            cancelAnimationFrame(engine.animFrameId);
            check(engine.renderErrors.size === 0, `V-Sync ${mode} failed.`);
            systems.push('V-Sync ' + mode);
        }
        engine.setVsyncMode('OFF', 0);
        engine.setMilkdropActive(true);
        engine.setMilkdropOpacity(0.3);
        for (let frame = 0; frame < 24; frame++) {
            engine.mainRenderLoop(now += 100);
            cancelAnimationFrame(engine.animFrameId);
            check(engine.renderErrors.size === 0, 'Milkdrop failed.');
        }
        check(engine.milkdropPresetDeck.length === 80, 'Milkdrop preset deck changed.');
        return { result: 'passed', voices, voiceFrames, systems, milkdropPresets: 80 };
    } finally {
        cancelAnimationFrame(engine.animFrameId);
        document.documentElement.style.filter = 'none';
        canvas.remove();
    }
}

window.runVoiceCarryoverChecks = runVoiceCarryoverChecks;
