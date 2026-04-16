import { clearAuth, getToken } from "../auth/authStorage";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000";

export class ApiError extends Error {
  constructor(message, status, payload) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.payload = payload;
  }
}

function getErrorMessage(payload, status) {
  if (payload?.message) {
    return payload.message;
  }

  if (payload?.errors && typeof payload.errors === "object") {
    for (const [field, messages] of Object.entries(payload.errors)) {
      if (Array.isArray(messages) && messages.length > 0) {
        return `${field}: ${messages[0]}`;
      }
    }
  }

  if (payload?.title) {
    return payload.title;
  }

  return `Request failed with status ${status}`;
}

function buildUrl(path, query) {
  const base = API_BASE_URL.endsWith("/") ? API_BASE_URL : `${API_BASE_URL}/`;
  const cleanPath = path.startsWith("/") ? path.slice(1) : path;
  const url = new URL(cleanPath, base);

  if (query) {
    Object.entries(query).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== "") {
        url.searchParams.set(key, String(value));
      }
    });
  }

  return url.toString();
}

export async function httpRequest(path, options = {}) {
  const { method = "GET", body, auth = false, query } = options;

  const headers = {
    Accept: "application/json"
  };

  if (body !== undefined) {
    headers["Content-Type"] = "application/json";
  }

  if (auth) {
    const token = getToken();
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }
  }

  const response = await fetch(buildUrl(path, query), {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined
  });

  const contentType = response.headers.get("content-type") ?? "";
  const hasJson = contentType.includes("application/json");
  const payload = hasJson ? await response.json() : null;

  if (!response.ok) {
    if (auth && response.status === 401) {
      clearAuth();

      if (typeof window !== "undefined" && window.location.pathname !== "/login") {
        window.location.assign("/login");
      }
    }

    const message = getErrorMessage(payload, response.status);
    throw new ApiError(message, response.status, payload);
  }

  return payload;
}
