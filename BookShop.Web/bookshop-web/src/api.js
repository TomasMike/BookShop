export async function responseError(response) {
    try {
        const body = await response.json();
        return body.error || body.message || body.detail || body.title || Object.values(body.errors || {}).flat().join(' ') || `Request failed (${response.status}).`;
    } catch {
        return `Request failed (${response.status}).`;
    }
}