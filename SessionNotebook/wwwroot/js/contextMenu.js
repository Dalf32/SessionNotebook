
function updateCaretPosition(textAreaId) {
    //console.log(window.getSelection()?.getRangeAt(0)?.getClientRects()?.[0]);
    const textArea = document.getElementById(textAreaId);

    let mirrorDiv = document.getElementById(textArea.nodeName + '--mirror-div');
    if (!mirrorDiv) {
        mirrorDiv = document.createElement('div');
        mirrorDiv.id = textArea.nodeName + '--mirror-div';
        document.body.appendChild(mirrorDiv);
    }

    const style = mirrorDiv.style;
    const computedStyle = getComputedStyle(textArea);
    Object.keys(style).forEach(prop => style[prop] = computedStyle[prop]);
    
    style.whiteSpace = 'pre-wrap';
    style.position = 'absolute';
    style.left = textArea.offsetLeft + parseInt(computedStyle.borderLeftWidth) + 'px';
    style.top = textArea.offsetTop + parseInt(computedStyle.borderTopWidth) + 'px';
    style.visibility = 'hidden';
    style.overflow = 'hidden';

    mirrorDiv.textContent = textArea.value.substring(0, textArea.selectionEnd);

    const span = document.createElement('span');

    span.textContent = textArea.value.substring(textArea.selectionEnd) || '.';
    mirrorDiv.appendChild(span);

    const x = span.offsetLeft + parseInt(computedStyle['borderLeftWidth']);
    const y = span.offsetTop + parseInt(computedStyle['borderTopWidth']);
    const elementRect = textArea.getBoundingClientRect();

    document.body.removeChild(mirrorDiv);

    document.documentElement.style.setProperty('--context-menu-x', `${elementRect.left + x}px`);
    document.documentElement.style.setProperty('--context-menu-y', `${elementRect.top + y}px`);
}

function getCaretIndex(textAreaId) {
    const textArea = document.getElementById(textAreaId);
    return textArea.selectionStart;
}
