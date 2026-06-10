import { marked } from 'https://cdn.jsdelivr.net/npm/marked@15/+esm';

export function copiarAlPortapapeles(texto) {
    return navigator.clipboard.writeText(texto);
}

export function renderizarMarkdown(markdown) {
    return marked.parse(markdown ?? '');
}

export function confirmar(mensaje) {
    return window.confirm(mensaje);
}
