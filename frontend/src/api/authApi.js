import { httpRequest } from "./httpClient";

export function loginRequest(credentials) {
  return httpRequest("/auth/login", {
    method: "POST",
    body: credentials
  });
}
