'use strict';

async function runNativeMediaChecks(videoUrl, audioUrl) {
    const check = (condition, message) => { if (!condition) throw new Error(message); };
    const engine = window.aiWarpEngine;
    if (!engine) throw new Error('Engine not initialized.');
    const video = document.getElementById('localVideo');
    video.crossOrigin = 'anonymous';
    video.src = videoUrl;
    video.loop = true;
    await video.play();
    check(video.videoWidth > 0 && video.videoHeight > 0, 'Converted video did not decode.');
    const sample = document.createElement('canvas');
    sample.width = 160;
    sample.height = 90;
    sample.getContext('2d').drawImage(video, 0, 0, 160, 90);
    const raw = sample.getContext('2d').getImageData(0, 0, 160, 90).data;
    check(raw.some((value, i) => i % 4 !== 3 && value > 100), 'Converted video pixels are missing/tainted.');
    engine.setSourceMode('video', video);
    engine.setWarpMode('kaleidoscope');
    engine.setAiWarpStrength(0.8);
    await new Promise(resolve => setTimeout(resolve, 500));
    const effected = engine.canvas.getContext('2d').getImageData(0, 0, engine.canvas.width, engine.canvas.height).data;
    check(effected.some((value, i) => i % 4 !== 3 && value > 100), 'Local-media FX canvas is unreadable/empty.');
    video.pause();
    video.currentTime = 0.3;
    await video.play();
    const audio = document.getElementById('localAudio');
    audio.crossOrigin = 'anonymous';
    audio.src = audioUrl;
    audio.loop = true;
    const context = new AudioContext();
    const source = context.createMediaElementSource(audio);
    const analyser = context.createAnalyser();
    source.connect(analyser);
    analyser.connect(context.destination);
    await context.resume();
    await audio.play();
    const spectrum = new Uint8Array(analyser.frequencyBinCount);
    let peak = 0;
    try {
        const deadline = performance.now() + 3000;
        while (peak === 0 && performance.now() < deadline) {
            await new Promise(resolve => setTimeout(resolve, 100));
            analyser.getByteFrequencyData(spectrum);
            peak = Math.max(...spectrum);
        }
        check(peak > 0, 'Converted audio analysis is silent after 3 seconds.');
    } finally {
        audio.pause();
        video.pause();
        await context.close();
    }
    return { result: 'passed', videoSize: [video.videoWidth, video.videoHeight],
        audioPeak: peak, pixelsReadable: true, transport: 'pause/seek/resume passed' };
}

window.runNativeMediaChecks = runNativeMediaChecks;
