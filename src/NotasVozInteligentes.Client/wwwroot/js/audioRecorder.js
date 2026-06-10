let mediaRecorder = null;
let chunks = [];
let inicio = 0;
let duracionSegundos = 0;
let mimeType = '';

export async function start() {
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    const preferido = ['audio/webm', 'audio/mp4', 'audio/ogg']
        .find(t => MediaRecorder.isTypeSupported(t));
    mediaRecorder = new MediaRecorder(stream, preferido ? { mimeType: preferido } : undefined);
    chunks = [];
    mediaRecorder.ondataavailable = e => { if (e.data.size > 0) chunks.push(e.data); };
    mediaRecorder.start();
    inicio = Date.now();
}

export function stop() {
    return new Promise((resolve, reject) => {
        if (!mediaRecorder) {
            reject(new Error('No hay grabación en curso.'));
            return;
        }
        mediaRecorder.onstop = async () => {
            duracionSegundos = (Date.now() - inicio) / 1000;
            // Gemini no acepta el parámetro codecs, solo el tipo base.
            mimeType = (mediaRecorder.mimeType || 'audio/webm').split(';')[0];
            const blob = new Blob(chunks, { type: mimeType });
            mediaRecorder.stream.getTracks().forEach(t => t.stop());
            mediaRecorder = null;
            resolve(new Uint8Array(await blob.arrayBuffer()));
        };
        mediaRecorder.stop();
    });
}

export function cancel() {
    if (!mediaRecorder) return;
    mediaRecorder.ondataavailable = null;
    mediaRecorder.stop();
    mediaRecorder.stream.getTracks().forEach(t => t.stop());
    mediaRecorder = null;
    chunks = [];
}

export function getMimeType() { return mimeType; }
export function getDuracionSegundos() { return duracionSegundos; }
