'use strict';

/** Error thrown by fetchJson for non-2xx responses or network failures. */
class ApiError extends Error {
    constructor(status, messages) {
        super(messages[0]);
        this.name = 'ApiError';
        this.status = status;
        this.messages = messages;
    }
}

/**
 * Sends a same-origin JSON request with the anti-forgery token header.
 * Objects passed as `body` are serialised to JSON. Resolves with the parsed
 * response body; rejects with an ApiError containing user-friendly messages.
 */
async function fetchJson(url, options = {}) {
    const { body, headers, ...rest } = options;
    const token = document.querySelector('meta[name="csrf-token"]')?.content;

    let response;
    try {
        response = await fetch(url, {
            credentials: 'same-origin',
            ...rest,
            headers: {
                Accept: 'application/json',
                ...(body !== undefined && { 'Content-Type': 'application/json' }),
                ...(token && { 'X-CSRF-TOKEN': token }),
                ...headers
            },
            body: body === undefined ? undefined : JSON.stringify(body)
        });
    } catch {
        throw new ApiError(0, ['Could not reach the server. Check your connection and try again.']);
    }

    const isJson = response.headers.get('Content-Type')?.includes('json');
    const data = isJson ? await response.json() : null;

    if (!response.ok) {
        throw new ApiError(response.status, errorMessages(data, response.status));
    }

    return data;
}

/** Extracts messages from ProblemDetails (errors as an array or a field dictionary). */
function errorMessages(problem, status) {
    if (problem?.errors) {
        return Array.isArray(problem.errors) ? problem.errors : Object.values(problem.errors).flat();
    }
    if (problem?.detail) {
        return [problem.detail];
    }
    if (problem?.title) {
        return [problem.title];
    }
    return [status === 400 ? 'The request was invalid. Please reload the page and try again.' : `Request failed (${status}).`];
}

/** Asks for confirmation before submitting any form with a data-confirm attribute. */
document.addEventListener('submit', (event) => {
    const message = event.target.dataset?.confirm;
    if (message && !window.confirm(message)) {
        event.preventDefault();
    }
});
