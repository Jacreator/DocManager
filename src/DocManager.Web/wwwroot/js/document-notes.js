'use strict';

(() => {
    const viewer = document.getElementById('document-viewer');
    if (!viewer) {
        return;
    }

    const FLIP_FORM_AFTER_PERCENT = 65;
    const layer = viewer.querySelector('[data-notes-layer]');
    const toggle = document.getElementById('annotate-toggle');
    const list = document.getElementById('notes-list');
    const emptyMessage = document.getElementById('notes-empty');
    const formTemplate = document.getElementById('note-form-template');
    const notesUrl = viewer.dataset.notesUrl;

    let noteCount = 0;
    let activeForm = null;

    JSON.parse(document.getElementById('notes-data').textContent).forEach(renderNote);

    toggle.addEventListener('change', () => {
        viewer.classList.toggle('is-annotating', toggle.checked);
        if (!toggle.checked) {
            closeForm();
        }
    });

    layer.addEventListener('click', (event) => {
        if (event.target === layer) {
            openForm(toPercentPosition(event));
        }
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
            closeForm();
        }
    });

    function toPercentPosition(event) {
        const rect = layer.getBoundingClientRect();
        return {
            top: toPercent(event.clientY - rect.top, rect.height),
            left: toPercent(event.clientX - rect.left, rect.width)
        };
    }

    function toPercent(offset, size) {
        const percent = (offset / size) * 100;
        return Math.round(Math.min(100, Math.max(0, percent)) * 100) / 100;
    }

    function placeAt(element, top, left) {
        element.style.top = `${top}%`;
        element.style.left = `${left}%`;
    }

    function openForm(position) {
        closeForm();

        const form = formTemplate.content.firstElementChild.cloneNode(true);
        placeAt(form, position.top, position.left);
        form.classList.toggle('note-form--flip', position.left > FLIP_FORM_AFTER_PERCENT);
        form.addEventListener('submit', (event) => {
            event.preventDefault();
            saveNote(form, position);
        });
        form.querySelector('[data-cancel]').addEventListener('click', closeForm);

        layer.appendChild(form);
        form.elements.noteText.focus();
        activeForm = form;
    }

    function closeForm() {
        activeForm?.remove();
        activeForm = null;
    }

    async function saveNote(form, position) {
        const noteText = form.elements.noteText.value.trim();
        if (!noteText) {
            showErrors(form, ['Please enter a note.']);
            return;
        }

        const submitButton = form.querySelector('[type="submit"]');
        submitButton.disabled = true;

        try {
            const note = await fetchJson(notesUrl, {
                method: 'POST',
                body: { noteText, positionTop: position.top, positionLeft: position.left }
            });
            renderNote(note);
            closeForm();
        } catch (error) {
            showErrors(form, error.messages ?? [error.message]);
            submitButton.disabled = false;
        }
    }

    function showErrors(form, messages) {
        const errorElement = form.querySelector('[data-error]');
        errorElement.textContent = messages.join(' ');
        errorElement.hidden = false;
    }

    function renderNote(note) {
        noteCount++;
        const label = String(noteCount);

        const marker = createElement('div', 'note-marker');
        marker.tabIndex = 0;
        marker.setAttribute('aria-label', `Note ${label}: ${note.noteText}`);
        placeAt(marker, note.positionTop, note.positionLeft);
        marker.append(
            createElement('span', 'note-marker__pin', label),
            createElement('span', 'note-marker__text', note.noteText));
        layer.appendChild(marker);

        const item = createElement('li', 'notes-list__item');
        item.append(
            createElement('span', 'note-marker__pin', label),
            createElement('span', 'notes-list__text', note.noteText));
        list.appendChild(item);
        emptyMessage.hidden = true;
    }

    /** Builds an element using textContent so user input is never interpreted as HTML. */
    function createElement(tag, className, text) {
        const element = document.createElement(tag);
        element.className = className;
        if (text !== undefined) {
            element.textContent = text;
        }
        return element;
    }
})();
