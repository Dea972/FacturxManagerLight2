/**
 * Déclenche le téléchargement d'un fichier binaire dans le navigateur via un Blob URL.
 * @param {string} fileName - Nom du fichier suggéré.
 * @param {string} contentType - Type MIME (ex : "application/pdf").
 * @param {Uint8Array} content - Contenu binaire du fichier.
 */
window.downloadFile = function (fileName, contentType, content) {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.style.display = 'none';
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
    setTimeout(() => URL.revokeObjectURL(url), 5000);
};
