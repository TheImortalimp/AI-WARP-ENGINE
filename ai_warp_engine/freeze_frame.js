(function (root) {
    'use strict';

    class FreezeFrameCapture {
        constructor(bridge) {
            this.bridge = bridge;
            this.pending = null;
            this.sequence = 0;
            if (bridge) bridge.addEventListener('message', event => {
                if (typeof event.data !== 'string' || !event.data.startsWith('freeze:result|')) return;
                try {
                    const result = JSON.parse(event.data.substring(14));
                    if (!this.pending || result.id !== this.pending.id) return;
                    if (result.error) this.pending.reject(new Error(result.error));
                    else if (typeof result.png === 'string' && /^[A-Za-z0-9+/]+={0,2}$/.test(result.png))
                        this.pending.resolve('data:image/png;base64,' + result.png);
                    else this.pending.reject(new Error('Invalid native screenshot response.'));
                } catch (error) {
                    if (this.pending) this.pending.reject(error);
                }
            });
        }

        async capture(canvas, sourceMode) {
            if (this.busy) throw new Error('A freeze-frame capture is already in progress.');
            this.busy = true;
            const page = document.documentElement;
            try {
                if (!canvas || !canvas.width || !canvas.height) throw new Error('The effects canvas is unavailable.');
                if (!this.bridge) {
                    if (sourceMode === 'youtube')
                        throw new Error('Share the YouTube tab/screen first to capture its picture and full FX.');
                    return await this.captureBrowser(canvas);
                }
                page.classList.add('freeze-frame-capture');
                await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
                return await new Promise((resolve, reject) => {
                    const id = String(++this.sequence);
                    this.pending = { id, resolve, reject };
                    this.timer = setTimeout(() => reject(new Error('Native screenshot capture timed out.')), 10000);
                    this.bridge.postMessage('freeze:capture|' + id);
                });
            } finally {
                clearTimeout(this.timer);
                this.pending = null;
                page.classList.remove('freeze-frame-capture');
                this.busy = false;
            }
        }

        async captureBrowser(canvas) {
            const out = document.createElement('canvas');
            out.width = canvas.width;
            out.height = canvas.height;
            const context = out.getContext('2d');
            if (!context) throw new Error('Screenshot Canvas 2D is unavailable.');
            context.fillStyle = '#020307';
            context.fillRect(0, 0, out.width, out.height);
            context.filter = getComputedStyle(canvas).filter;
            context.drawImage(canvas, 0, 0);
            context.filter = 'none';
            const layer = document.getElementById('fullscreen-fx-layer');
            if (layer && getComputedStyle(layer).display !== 'none' && getComputedStyle(layer).visibility !== 'hidden') {
                const width = innerWidth, height = innerHeight;
                const wrapper = document.createElement('div');
                wrapper.setAttribute('xmlns', 'http://www.w3.org/1999/xhtml');
                wrapper.style.cssText = `position:relative;width:${width}px;height:${height}px;overflow:hidden`;
                for (const element of layer.querySelectorAll('.crt-overlay, .cyber-vignette')) {
                    const clone = element.cloneNode(false);
                    const style = getComputedStyle(element);
                    for (const name of ['position', 'inset', 'background-image', 'background-size',
                        'background-color', 'box-shadow', 'opacity', 'display', 'visibility']) {
                        clone.style.setProperty(name, style.getPropertyValue(name));
                    }
                    wrapper.appendChild(clone);
                }
                const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}">` +
                    `<foreignObject width="100%" height="100%">${new XMLSerializer().serializeToString(wrapper)}</foreignObject></svg>`;
                const image = new Image();
                await new Promise((resolve, reject) => {
                    image.onload = resolve;
                    image.onerror = () => reject(new Error('Could not rasterize fullscreen screenshot effects.'));
                    image.src = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg);
                });
                context.globalAlpha = Number(getComputedStyle(layer).opacity);
                context.drawImage(image, 0, 0, out.width, out.height);
                context.globalAlpha = 1;
            }
            const final = document.createElement('canvas');
            final.width = out.width;
            final.height = out.height;
            const finalContext = final.getContext('2d');
            if (!finalContext) throw new Error('Final screenshot Canvas 2D is unavailable.');
            finalContext.filter = getComputedStyle(document.documentElement).filter;
            finalContext.drawImage(out, 0, 0);
            return final.toDataURL('image/png');
        }
    }

    root.FreezeFrameCapture = FreezeFrameCapture;
})(window);
