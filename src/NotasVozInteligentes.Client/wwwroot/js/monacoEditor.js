const VS = 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.2/min/vs';
let editor = null;

function cargarMonaco() {
    if (window.monaco) return Promise.resolve();
    if (window.__monacoCargando) return window.__monacoCargando;
    window.__monacoCargando = new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = `${VS}/loader.js`;
        script.onload = () => {
            window.require.config({ paths: { vs: VS } });
            window.require(['vs/editor/editor.main'], resolve);
        };
        script.onerror = () => reject(new Error('No se pudo cargar Monaco Editor.'));
        document.head.appendChild(script);
    });
    return window.__monacoCargando;
}

export async function create(elementId, valorInicial, dotnetRef) {
    await cargarMonaco();
    dispose();
    editor = window.monaco.editor.create(document.getElementById(elementId), {
        value: valorInicial,
        language: 'markdown',
        wordWrap: 'on',
        minimap: { enabled: false },
        automaticLayout: true,
        scrollBeyondLastLine: false
    });
    let timeout = null;
    editor.onDidChangeModelContent(() => {
        clearTimeout(timeout);
        timeout = setTimeout(() => dotnetRef.invokeMethodAsync('OnContenidoCambiado'), 400);
    });
}

export function getValue() { return editor?.getValue() ?? ''; }
export function setValue(valor) { editor?.setValue(valor); }

export function dispose() {
    editor?.dispose();
    editor = null;
}
