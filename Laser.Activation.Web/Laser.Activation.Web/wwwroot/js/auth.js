export async function copyToClipboard(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch {
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.left = '-999999px';
        textArea.style.top = '-999999px';
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        try {
            document.execCommand('copy');
            document.body.removeChild(textArea);
            return true;
        } catch {
            document.body.removeChild(textArea);
            return false;
        }
    }
}

export function saveToken(token) {
    localStorage.setItem('authToken', token);
}

export function getToken() {
    return localStorage.getItem('authToken') || '';
}

export function removeToken() {
    localStorage.removeItem('authToken');
}

export function isLoggedIn() {
    const token = getToken();
    if (!token) return false;
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        return payload.exp * 1000 > Date.now();
    } catch {
        return false;
    }
}

function decodePayload() {
    const token = getToken();
    if (!token) return null;
    try {
        return JSON.parse(atob(token.split('.')[1]));
    } catch {
        return null;
    }
}

export function getUserId() {
    return decodePayload()?.userId || '';
}

export function getUserName() {
    return decodePayload()?.username || '';
}

export function getUserRole() {
    return decodePayload()?.role || '';
}

export async function loginUser(username, password) {
    try {
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password })
        });
        return await response.json();
    } catch (e) {
        return { success: false, message: '网络错误: ' + e.message };
    }
}

export async function apiGet(url) {
    const token = getToken();
    const response = await fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    });
    if (response.status === 401) {
        removeToken();
        window.location.href = '/login';
        return null;
    }
    return await response.json();
}

export async function apiDelete(url) {
    const token = getToken();
    const response = await fetch(url, {
        method: 'DELETE',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    });
    if (response.status === 401) {
        removeToken();
        window.location.href = '/login';
        return null;
    }
    return await response.json();
}

export async function apiPostForm(url, formData) {
    const token = getToken();
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Authorization': 'Bearer ' + token
        },
        body: formData
    });
    if (response.status === 401) {
        removeToken();
        window.location.href = '/login';
        return null;
    }
    return await response.json();
}

export async function apiDownload(url) {
    const token = getToken();
    const response = await fetch(url, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    });
    if (response.status === 401) {
        removeToken();
        window.location.href = '/login';
        return;
    }
    const blob = await response.blob();
    const contentDisposition = response.headers.get('Content-Disposition');
    let filename = 'activation.reqrep';
    if (contentDisposition) {
        const match = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
        if (match && match[1]) {
            filename = match[1].replace(/['"]/g, '');
        }
    }
    const downloadUrl = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = downloadUrl;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(downloadUrl);
}

export async function activateDevice(base64File, fileName, projectName, departmentName, personName, versionInf) {
    const token = getToken();
    const binaryString = atob(base64File);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i);
    }
    const blob = new Blob([bytes]);
    const formData = new FormData();
    formData.append('hwidFile', blob, fileName);
    formData.append('projectName', projectName);
    formData.append('departmentName', departmentName);
    formData.append('personName', personName);
    formData.append('versionInf', versionInf);

    try {
        const response = await fetch('/api/activation/activate', {
            method: 'POST',
            headers: {
                'Authorization': 'Bearer ' + token
            },
            body: formData
        });
        if (response.status === 401) {
            removeToken();
            window.location.href = '/login';
            return { success: false, message: '登录已过期，请重新登录' };
        }
        return await response.json();
    } catch (e) {
        return { success: false, message: '网络错误: ' + e.message };
    }
}
