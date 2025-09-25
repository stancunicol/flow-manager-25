window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

window.focusElement = (element) => {
    if (element) {
        element.focus();
    }
};

window.autoResizeTextarea = (element) => {
    if (element) {
        element.style.height = 'auto';
        element.style.height = element.scrollHeight + 'px';
    }
};